// Instanced grass clumps and wild flowers (NatureGrass draws them with RenderMeshInstanced). Vertex
// colour: r = height along the blade (0 root, 1 tip), g = random per blade, b = petal. Each blade
// roots in the colour of the ground beneath it (same noise as Stylized Ground) and lightens to
// its tip; the wind bends it from the root in rolling gusts, and the sun shines through the tips
// when it is behind them.
Shader "Nemequene/Stylized Grass"
{
    Properties
    {
        _GrassA ("Ground grass", Color) = (0.36, 0.50, 0.22, 1)
        _GrassB ("Ground grass light", Color) = (0.52, 0.62, 0.28, 1)
        _Dry ("Dry straw", Color) = (0.70, 0.64, 0.36, 1)
        _DryAmount ("Dry amount", Range(0, 1)) = 0.45
        _Tip ("Tip", Color) = (0.70, 0.80, 0.36, 1)
        _TipDry ("Tip dry", Color) = (0.86, 0.80, 0.48, 1)
        _FlowerA ("Flower A", Color) = (0.98, 0.84, 0.25, 1)
        _FlowerB ("Flower B", Color) = (0.96, 0.95, 0.88, 1)
        _FlowerC ("Flower C", Color) = (0.66, 0.46, 0.86, 1)
        _ShadeTint ("Shade tint", Color) = (0.78, 0.86, 1.05, 1)
        _WindStrength ("Wind strength", Float) = 0.35
        _WindSpeed ("Wind speed", Float) = 1
        _Translucency ("Translucency", Range(0, 1)) = 0.45
        _NoiseScale ("Noise scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        Cull Off

        HLSLINCLUDE
        #include "NatureCommon.hlsl"

        CBUFFER_START(UnityPerMaterial)
        half4 _GrassA, _GrassB, _Dry, _Tip, _TipDry, _FlowerA, _FlowerB, _FlowerC, _ShadeTint;
        float _DryAmount, _WindStrength, _WindSpeed, _Translucency, _NoiseScale;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            half4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 rootWS : TEXCOORD1;
            half4 color : TEXCOORD2;
            float fog : TEXCOORD3;
        };

        Varyings Vert(Attributes input)
        {
            UNITY_SETUP_INSTANCE_ID(input);
            Varyings o;
            float3 root = TransformObjectToWorld(float3(0, 0, 0));
            float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
            float h = input.color.r;
            float3 bend = NatureWind(root + input.color.g * 2, _WindStrength, _WindSpeed) * h * h;
            positionWS.xz += bend.xz;
            positionWS.y -= length(bend.xz) * h * .4;
            o.positionWS = positionWS;
            o.rootWS = root;
            o.color = input.color;
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

            half4 Frag(Varyings i) : SV_Target
            {
                float2 p = i.rootWS.xz * _NoiseScale;
                float h = i.color.r;
                // The ground's own colour at the root, darker in the clump's heart.
                half3 ground = lerp(_GrassA.rgb, _GrassB.rgb, smoothstep(.3, .72, NatureFbm(p * .045)));
                float dry = smoothstep(.52, .74, NatureFbm(p * .07 + 21.7)) * _DryAmount;
                ground = lerp(ground, _Dry.rgb, dry);
                half3 tip = lerp(_Tip.rgb, _TipDry.rgb, saturate(dry * 1.3 + (i.color.g - .5) * .5));
                half3 color = lerp(ground * .72, tip, pow(h, .8));
                color *= lerp(.9, 1.1, i.color.g);

                // Petals: one of three wild colours per flower.
                float pick = NatureHash(i.rootWS.xz * 3.1 + i.color.g * 17);
                half3 petal = pick < .45 ? _FlowerA.rgb : pick < .75 ? _FlowerB.rgb : _FlowerC.rgb;
                color = lerp(color, petal, step(.5, i.color.b));

                half3 up = half3(0, 1, 0);
                half3 lit = NatureLight(color, i.positionWS, up, .5, _ShadeTint.rgb);
                // Sun through the blades when looking towards it.
                Light sun = GetMainLight();
                float3 view = normalize(GetWorldSpaceViewDir(i.positionWS));
                half through = pow(saturate(dot(-view, sun.direction)), 4) * h * _Translucency;
                lit += sun.color * tip * through;
                return half4(MixFog(lit, i.fog), 1);
            }
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
            half4 NormalFrag(Varyings i) : SV_Target { return NatureDepthNormal(float3(0, 1, 0)); }
            ENDHLSL
        }
    }
}
