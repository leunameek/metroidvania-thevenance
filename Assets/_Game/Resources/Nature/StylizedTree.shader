// Trees and shrubs built from code (NatureTrees), drawn instanced. Vertex colour: r = leaf (0 bark,
// 1 leaf), g = random per leaf or branch, b = openness (0 deep inside the crown, 1 at its skin),
// a = flexibility (0 at the foot of the trunk, 1 at the twig tips). Leaf normals point out of the
// crown, so a whole canopy shades as one soft mass; the wind sways each tree from its foot, more
// at the tips, and the leaves flutter on their own; the sun shines through the leaves behind them.
Shader "Nemequene/Stylized Tree"
{
    Properties
    {
        _Bark ("Bark", Color) = (0.42, 0.33, 0.25, 1)
        _BarkDark ("Bark dark", Color) = (0.24, 0.18, 0.13, 1)
        _LeafDark ("Leaf inner", Color) = (0.13, 0.26, 0.12, 1)
        _LeafLight ("Leaf outer", Color) = (0.42, 0.58, 0.22, 1)
        _LeafAccent ("Leaf accent (young leaves, flowers, berries)", Color) = (0.70, 0.28, 0.18, 1)
        _AccentAmount ("Accent amount", Range(0, 1)) = 0
        _ShadeTint ("Shade tint", Color) = (0.74, 0.84, 1.05, 1)
        _WindStrength ("Wind strength", Float) = 0.25
        _WindSpeed ("Wind speed", Float) = 0.8
        _Flutter ("Leaf flutter", Float) = 0.025
        _Translucency ("Translucency", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        Cull Off

        HLSLINCLUDE
        #include "NatureCommon.hlsl"

        CBUFFER_START(UnityPerMaterial)
        half4 _Bark, _BarkDark, _LeafDark, _LeafLight, _LeafAccent, _ShadeTint;
        float _AccentAmount, _WindStrength, _WindSpeed, _Flutter, _Translucency;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            half4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            half4 color : TEXCOORD2;
            float fog : TEXCOORD3;
            float3 rootWS : TEXCOORD4;
        };

        float3 Sway(float3 positionWS, float3 root, half4 color)
        {
            float flex = color.a;
            // The whole tree leans with the gust (its own phase from where it stands); the twigs
            // whip a little later than the trunk.
            float3 push = NatureWind(root, _WindStrength, _WindSpeed);
            float3 lag = NatureWind(root + float3(1.7, 0, 2.3) + color.g, _WindStrength, _WindSpeed * 1.3);
            float3 bend = lerp(push, lag, saturate(flex * 1.2 - .3)) * flex * flex;
            positionWS.xz += bend.xz;
            positionWS.y -= length(bend.xz) * flex * .25;
            // Leaves shiver on their own.
            float leaf = color.r * flex;
            float t = _Time.y * (6 + color.g * 4) + color.g * 40;
            positionWS += float3(sin(t), sin(t * 1.3 + 1), cos(t * .9)) * _Flutter * leaf;
            return positionWS;
        }

        Varyings Vert(Attributes input)
        {
            UNITY_SETUP_INSTANCE_ID(input);
            Varyings o;
            float3 root = TransformObjectToWorld(float3(0, 0, 0));
            float3 positionWS = Sway(TransformObjectToWorld(input.positionOS.xyz), root, input.color);
            o.positionWS = positionWS;
            o.normalWS = TransformObjectToWorldNormal(input.normalOS);
            o.color = input.color;
            o.rootWS = root;
            o.positionCS = TransformWorldToHClip(positionWS);
            o.fog = ComputeFogFactor(o.positionCS.z);
            return o;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog

            half4 Frag(Varyings i, bool front : SV_IsFrontFace) : SV_Target
            {
                half open = i.color.b, g = i.color.g;
                half3 n = SafeNormalize(i.normalWS);
                half3 color;
                half wrap;
                if (i.color.r > .5)
                {
                    // Leaves: dark in the heart of the crown, light at its skin, each a little
                    // different; a share of them takes the accent (young red leaves, flowers, berries).
                    color = lerp(_LeafDark.rgb, _LeafLight.rgb, saturate(open * .85 + (g - .5) * .35));
                    float tree = NatureHash(i.rootWS.xz * .37);
                    color *= lerp(.88, 1.1, tree);
                    color = lerp(color, _LeafAccent.rgb, step(1 - _AccentAmount, frac(g * 7.31 + tree)));
                    wrap = .6;
                }
                else
                {
                    // Bark: streaks along the trunk, darker low down and inside the crown.
                    float streak = NatureNoise(float2(atan2(n.z, n.x) * 3, i.positionWS.y * 1.5 + g * 9));
                    color = lerp(_BarkDark.rgb, _Bark.rgb, saturate(streak * .9 + open * .4));
                    if (!front) n = -n;
                    wrap = .25;
                }
                half3 lit = NatureLight(color, i.positionWS, n, wrap, _ShadeTint.rgb);
                // Inner leaves and branches get less of the sky.
                lit *= lerp(.55, 1, open);
                if (i.color.r > .5)
                {
                    Light sun = GetMainLight();
                    float3 view = normalize(GetWorldSpaceViewDir(i.positionWS));
                    half through = pow(saturate(dot(-view, sun.direction)), 3) * _Translucency * (.4 + .6 * open);
                    lit += sun.color * _LeafLight.rgb * through;
                }
                return half4(NatureFinite(MixFog(lit, i.fog)), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float4 ShadowVert(Attributes input) : SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 root = TransformObjectToWorld(float3(0, 0, 0));
                float3 positionWS = Sway(TransformObjectToWorld(input.positionOS.xyz), root, input.color);
                return NatureShadowClip(positionWS, TransformObjectToWorldNormal(input.normalOS));
            }
            half4 ShadowFrag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing
            half4 DepthFrag(Varyings i) : SV_Target { return i.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment NormalFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            half4 NormalFrag(Varyings i) : SV_Target { return NatureDepthNormal(i.normalWS); }
            ENDHLSL
        }
    }
}
