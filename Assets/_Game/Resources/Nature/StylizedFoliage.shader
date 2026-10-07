// Textured plants (reeds, frailejones, trees and bushes from Tripo) that sway in the same wind as
// the grass. The sway grows with the height above the plant's base: _SwayBase and _SwayHeight are
// set per renderer by NatureFoliage, so the trunk stays planted and the crown moves.
Shader "Nemequene/Stylized Foliage"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _ShadeTint ("Shade tint", Color) = (0.80, 0.88, 1.05, 1)
        _WindStrength ("Wind strength", Float) = 0.12
        _WindSpeed ("Wind speed", Float) = 1
        _Flutter ("Leaf flutter", Float) = 0.015
        _SwayBase ("Sway base (world y)", Float) = 0
        _SwayHeight ("Sway height", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #include "NatureCommon.hlsl"

        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor, _ShadeTint;
        float _WindStrength, _WindSpeed, _Flutter, _SwayBase, _SwayHeight;
        CBUFFER_END

        struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float2 uv : TEXCOORD2;
            float fog : TEXCOORD3;
        };

        float3 Sway(float3 positionWS)
        {
            float3 origin = TransformObjectToWorld(float3(0, 0, 0));
            float h = saturate((positionWS.y - _SwayBase) / max(_SwayHeight, .01));
            float3 bend = NatureWind(float3(origin.x, 0, origin.z), _WindStrength, _WindSpeed) * h * h;
            float t = _Time.y * _WindSpeed;
            float3 flutter = float3(sin(t * 7.1 + positionWS.y * 4.3 + positionWS.x * 2.1), sin(t * 9.3 + positionWS.z * 3.7) * .6, sin(t * 6.4 + positionWS.x * 3.3)) * _Flutter * h;
            positionWS += bend * _SwayHeight + flutter;
            return positionWS;
        }

        Varyings Vert(Attributes input)
        {
            Varyings o;
            o.positionWS = Sway(TransformObjectToWorld(input.positionOS.xyz));
            o.normalWS = TransformObjectToWorldNormal(input.normalOS);
            o.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            o.positionCS = TransformWorldToHClip(o.positionWS);
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            half4 Frag(Varyings i) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;
                half3 lit = NatureLight(albedo, i.positionWS, SafeNormalize(i.normalWS), .4, _ShadeTint.rgb);
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
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            float4 ShadowVert(Attributes input) : SV_POSITION
            {
                return NatureShadowClip(Sway(TransformObjectToWorld(input.positionOS.xyz)), TransformObjectToWorldNormal(input.normalOS));
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
