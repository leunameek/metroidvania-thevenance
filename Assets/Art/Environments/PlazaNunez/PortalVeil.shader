Shader "Nemequene/Portal Veil"
{
    Properties
    {
        [MainColor] _BaseColor("Portal color", Color) = (0.2,0.6,0.8,1)
        _EmissionColor("Energy", Color) = (0.2,0.6,0.8,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 localPoint : TEXCOORD0; float fog : TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EmissionColor;
            CBUFFER_END
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.localPoint = input.positionOS.xy * 2;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float r = length(input.localPoint);
                float angle = atan2(input.localPoint.y, input.localPoint.x);
                float spiral = pow(saturate(sin(r * 24 - angle * 3 - _Time.y * 1.8) * 0.5 + 0.5), 5);
                float rim = pow(saturate(r), 5);
                float heart = pow(saturate(1 - r), 3);
                half3 color = _BaseColor.rgb * (0.16 + spiral * 0.3)
                    + _EmissionColor.rgb * (0.28 + rim * 0.85 + heart * 0.45 + spiral * 0.35);
                return half4(MixFog(color, input.fog), 1);
            }
            ENDHLSL
        }
    }
}
