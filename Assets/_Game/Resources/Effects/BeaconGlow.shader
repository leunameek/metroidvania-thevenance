// The glow of something the player should go to (Beacon): drawn over the object's own mesh,
// additive, a faint fill that brightens toward the rims and breathes with _Intensity. The mesh is
// pushed out a hair along its normals so it never fights with the surface under it.
Shader "Nemequene/Beacon Glow"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.78, 0.4, 1)
        _Intensity ("Intensity", Float) = 1
        _Inflate ("Inflate (m)", Float) = 0.006
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+40" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Glow"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back ZWrite Off ZTest LEqual Blend One One
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float _Intensity, _Inflate;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };
            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.positionWS = TransformObjectToWorld(i.positionOS.xyz) + SafeNormalize(o.normalWS) * _Inflate;
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float3 v = SafeNormalize(_WorldSpaceCameraPos - i.positionWS);
                float rim = pow(1 - saturate(abs(dot(SafeNormalize(i.normalWS), v))), 2.2);
                half3 c = _Color.rgb * max(_Intensity, 0) * (.18 + rim * 1.5);
                return half4(min(c, 4), 1);
            }
            ENDHLSL
        }
    }
}
