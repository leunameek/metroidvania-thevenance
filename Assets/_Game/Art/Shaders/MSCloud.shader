// Stylised cloud of the Mundo Superior: soft wrap lighting from the sun between a warm lit cream
// and a cool turquoise shade, a bright rim on the silhouette, and a slow "breathing" of the puffs
// along their normals. Unlit (no shadows cast or received) so clouds never darken a landing.
Shader "Nemequene/Cloud"
{
    Properties
    {
        _LitColor ("Lit", Color) = (1, 0.97, 0.9, 1)
        _ShadeColor ("Shade", Color) = (0.74, 0.86, 0.88, 1)
        _RimColor ("Rim", Color) = (1, 1, 1, 1)
        _RimPower ("Rim power", Range(0.5, 8)) = 2.6
        _RimStrength ("Rim strength", Range(0, 1)) = 0.55
        _Wobble ("Breathing (m)", Range(0, 0.6)) = 0.15
        _WobbleSpeed ("Breathing speed", Range(0, 3)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _LitColor, _ShadeColor, _RimColor;
            float _RimPower, _RimStrength, _Wobble, _WobbleSpeed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float fog : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings o;
                float3 ws = TransformObjectToWorld(input.positionOS.xyz);
                float3 n = TransformObjectToWorldNormal(input.normalOS);
                // Each puff swells and settles slightly out of phase with its neighbours.
                float w = sin(_Time.y * _WobbleSpeed + ws.x * 0.31 + ws.z * 0.23 + ws.y * 0.17);
                ws += n * w * _Wobble;
                o.positionWS = ws;
                o.normalWS = n;
                o.positionCS = TransformWorldToHClip(ws);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 n = normalize(input.normalWS);
                Light sun = GetMainLight();
                float wrap = dot(n, sun.direction) * 0.5 + 0.5;
                float top = n.y * 0.5 + 0.5;
                half3 c = lerp(_ShadeColor.rgb, _LitColor.rgb, saturate(wrap * 0.65 + top * 0.55));
                float3 view = normalize(GetWorldSpaceViewDir(input.positionWS));
                float rim = pow(1 - saturate(dot(n, view)), _RimPower);
                c = lerp(c, _RimColor.rgb, rim * _RimStrength);
                c = MixFog(c, input.fog);
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
