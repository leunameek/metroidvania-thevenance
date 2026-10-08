// The column of light over something the player should go to (Beacon), seen from afar: a soft
// additive cylinder (uv.x around, uv.y up), brightest at its middle and fading at its edges and
// toward the top, with bands and motes of light rising through it.
Shader "Nemequene/Beacon Beam"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.78, 0.4, 1)
        _Intensity ("Intensity", Float) = 1
        _Height ("Height (m)", Float) = 6
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+30" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Beam"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off ZWrite Off ZTest LEqual Blend One One
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float _Intensity, _Height;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; float2 uv : TEXCOORD2; };
            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.uv = i.uv;
                return o;
            }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            half4 Frag(Varyings i) : SV_Target
            {
                float3 v = SafeNormalize(_WorldSpaceCameraPos - i.positionWS);
                float3 n = SafeNormalize(i.normalWS);
                // Seen through its middle it glows; its silhouette melts away.
                float body = pow(saturate(abs(dot(n, v))), 1.6);
                float h = i.uv.y;
                float fade = smoothstep(0, .06, h) * pow(saturate(1 - h), 1.8);
                float meters = h * _Height;
                float bands = .55 + .45 * sin((meters * 1.1 - _Time.y * 1.4) * 6.2832);
                // Motes rising on the column.
                float2 grid = float2(i.uv.x * 22, meters * 2.2 - _Time.y * 1.3);
                float2 cell = floor(grid), local = frac(grid) - .5;
                float seed = Hash(cell);
                float mote = step(.72, seed) * smoothstep(.22, 0, length(local + (float2(Hash(cell + 3.1), Hash(cell + 7.7)) - .5) * .5));
                float glow = body * fade * (.22 + .2 * bands) + mote * fade * (.6 + .4 * body);
                half3 c = _Color.rgb * max(_Intensity, 0) * glow;
                return half4(min(c, 4), 1);
            }
            ENDHLSL
        }
    }
}
