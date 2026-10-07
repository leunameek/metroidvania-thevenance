// Painted ground of the open places (Bacatá, the hill, the lagoon of Iguaque): two greens mixed
// by large noise, dry straw patches, fine speckle, earth on steep slopes, wet mud and sand at the
// shore line, and dirt paths and clearings drawn from segments and circles set by script
// (NatureGround). No textures: it reads the same at any distance and never tiles.
Shader "Nemequene/Stylized Ground"
{
    Properties
    {
        _GrassA ("Grass", Color) = (0.36, 0.50, 0.22, 1)
        _GrassB ("Grass light", Color) = (0.52, 0.62, 0.28, 1)
        _Dry ("Dry straw", Color) = (0.70, 0.64, 0.36, 1)
        _DryAmount ("Dry amount", Range(0, 1)) = 0.45
        _Dirt ("Dirt", Color) = (0.56, 0.44, 0.30, 1)
        _DirtDark ("Dirt worn", Color) = (0.45, 0.34, 0.23, 1)
        _Slope ("Steep earth", Color) = (0.50, 0.42, 0.30, 1)
        _Shore ("Shore mud", Color) = (0.42, 0.36, 0.26, 1)
        _ShoreLevel ("Water level", Float) = -100
        _ShadeTint ("Shade tint", Color) = (0.78, 0.86, 1.05, 1)
        _NoiseScale ("Noise scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #include "NatureCommon.hlsl"

        half4 _GrassA, _GrassB, _Dry, _Dirt, _DirtDark, _Slope, _Shore, _ShadeTint;
        float _DryAmount, _ShoreLevel, _NoiseScale;
        float4 _Paths[8];      // xy start, zw end (world xz)
        float4 _PathInfo[8];   // x width, y strength
        float4 _Clearings[16]; // xy centre, z radius, w strength
        float _PathCount, _ClearingCount;

        struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float fog : TEXCOORD2;
        };

        Varyings Vert(Attributes input)
        {
            Varyings o;
            o.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            o.normalWS = TransformObjectToWorldNormal(input.normalOS);
            o.positionCS = TransformWorldToHClip(o.positionWS);
            o.fog = ComputeFogFactor(o.positionCS.z);
            return o;
        }

        float DirtMask(float2 p, float edge)
        {
            float m = 0;
            for (int i = 0; i < 8; i++)
            {
                if (i >= (int)_PathCount) break;
                float2 a = _Paths[i].xy, b = _Paths[i].zw;
                float2 pa = p - a, ba = b - a;
                float h = saturate(dot(pa, ba) / max(dot(ba, ba), 1e-4));
                float d = length(pa - ba * h) + edge;
                float w = _PathInfo[i].x * .5;
                m = max(m, (1 - smoothstep(w * .5, w * 1.2, d)) * _PathInfo[i].y);
            }
            for (int j = 0; j < 16; j++)
            {
                if (j >= (int)_ClearingCount) break;
                float d = length(p - _Clearings[j].xy) + edge * 1.6;
                float r = _Clearings[j].z;
                m = max(m, (1 - smoothstep(r * .55, r * 1.05, d)) * _Clearings[j].w);
            }
            return m;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog

            half4 Frag(Varyings i) : SV_Target
            {
                float2 p = i.positionWS.xz * _NoiseScale;
                half3 n = normalize(i.normalWS);
                float large = NatureFbm(p * .045);
                float mid = NatureFbm(p * .19 + 7.3);
                float fine = NatureNoise(p * 1.7);
                float speck = NatureNoise(p * 6.3);

                half3 grass = lerp(_GrassA.rgb, _GrassB.rgb, smoothstep(.3, .72, large));
                float dry = smoothstep(.52, .74, NatureFbm(p * .07 + 21.7)) * _DryAmount;
                grass = lerp(grass, _Dry.rgb, dry);
                grass *= lerp(.9, 1.08, fine) * lerp(.95, 1.05, speck);

                // Earth on the steep faces of the hills and the cerros.
                float steep = smoothstep(.86, .66, n.y);
                half3 color = lerp(grass, _Slope.rgb * lerp(.9, 1.1, mid), steep * .8);

                // Paths and clearings: trodden earth with a ragged, grassy edge.
                float dirt = DirtMask(i.positionWS.xz, (mid - .5) * 1.4);
                half3 earth = lerp(_Dirt.rgb, _DirtDark.rgb, smoothstep(.35, .7, mid)) * lerp(.92, 1.06, speck);
                color = lerp(color, earth, saturate(dirt));

                // Shore: damp mud just above the water, darker below it.
                float above = i.positionWS.y - _ShoreLevel;
                float shore = 1 - smoothstep(.05, .45 + mid * .25, above);
                color = lerp(color, _Shore.rgb * lerp(.9, 1.05, fine), shore);
                color *= 1.0 - .38 * saturate(-above * 1.5);

                half3 lit = NatureLight(color, i.positionWS, n, .35, _ShadeTint.rgb);
                return half4(MixFog(lit, i.fog), 1);
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
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float4 ShadowVert(Attributes input) : SV_POSITION
            {
                return NatureShadowClip(TransformObjectToWorld(input.positionOS.xyz), TransformObjectToWorldNormal(input.normalOS));
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
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            half4 NormalFrag(Varyings i) : SV_Target { return NatureDepthNormal(i.normalWS); }
            ENDHLSL
        }
    }
}
