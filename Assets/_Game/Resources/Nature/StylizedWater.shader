// Still mountain water (the lagoon of Iguaque): the bed seen through it and bent by the ripples,
// turning teal and then deep blue with depth, the sky reflected at grazing angles, sun glints,
// shadows on the surface and a soft foam line where it meets the shore and anything standing in
// it. Reads the camera depth and opaque textures (both on in the PC renderer).
Shader "Nemequene/Stylized Water"
{
    Properties
    {
        _ShallowColor ("Shallow tint", Color) = (0.62, 0.86, 0.80, 1)
        _DeepColor ("Deep", Color) = (0.05, 0.22, 0.28, 1)
        _Clarity ("Clarity (m)", Float) = 1.1
        _SkyColor ("Sky reflection", Color) = (0.50, 0.72, 0.90, 1)
        _HorizonColor ("Horizon reflection", Color) = (0.84, 0.90, 0.92, 1)
        _Reflection ("Reflection", Range(0, 1)) = 0.6
        _FoamColor ("Foam", Color) = (0.95, 0.97, 0.92, 1)
        _FoamDepth ("Foam depth (m)", Float) = 0.35
        _Refraction ("Refraction", Float) = 0.035
        _RippleScale ("Ripple scale", Float) = 1
        _RippleStrength ("Ripple strength", Float) = 0.22
        _FlowSpeed ("Flow speed", Float) = 0.25
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-50" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #include "NatureCommon.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _ShallowColor, _DeepColor, _SkyColor, _HorizonColor, _FoamColor;
            float _Clarity, _Reflection, _FoamDepth, _Refraction, _RippleScale, _RippleStrength, _FlowSpeed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fog : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                float3 p = TransformObjectToWorld(input.positionOS.xyz);
                p.y += (sin(p.x * .7 + _Time.y * 1.1) + sin(p.z * .9 - _Time.y * .8)) * .012;
                o.positionWS = p;
                o.positionCS = TransformWorldToHClip(p);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            float Ripples(float2 p)
            {
                float t = _Time.y * _FlowSpeed;
                return NatureNoise(p * .55 + float2(t, t * .6)) * .5
                     + NatureNoise(p * 1.6 + float2(-t * 1.3, t)) * .32
                     + NatureNoise(p * 4.3 + float2(t * 2.1, -t * 1.7)) * .18;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 p = i.positionWS.xz * _RippleScale;
                const float e = .04;
                float h = Ripples(p);
                float3 n = normalize(float3(-(Ripples(p + float2(e, 0)) - h) / e * _RippleStrength, 1, -(Ripples(p + float2(0, e)) - h) / e * _RippleStrength));
                float3 view = normalize(GetWorldSpaceViewDir(i.positionWS));

                // Depth of water along the view, then the bed bent by the ripples.
                float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
                float surface = LinearEyeDepth(i.positionWS, GetWorldToViewMatrix());
                float depth = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams) - surface;
                float2 bentUV = uv + n.xz * _Refraction * saturate(depth);
                float bentDepth = LinearEyeDepth(SampleSceneDepth(bentUV), _ZBufferParams) - surface;
                if (bentDepth < 0) { bentUV = uv; bentDepth = depth; }
                half3 bed = SampleSceneColor(bentUV);
                float murk = 1 - exp(-max(bentDepth, 0) / max(_Clarity, .01));
                half3 water = lerp(bed * _ShallowColor.rgb, _DeepColor.rgb, murk);

                // The sky on the surface, strongest at grazing angles.
                float3 r = reflect(-view, n);
                half3 sky = lerp(_HorizonColor.rgb, _SkyColor.rgb, saturate(r.y * 2.2));
                float fresnel = .04 + .96 * pow(1 - saturate(dot(n, view)), 5);
                water = lerp(water, sky, saturate(fresnel * _Reflection + .08));

                // Sun glints and the shadows of whoever stands by the water.
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half shadow = half(1.0) - half(0.6) * (half(1.0) - sun.shadowAttenuation);
                water *= half(0.75) + half(0.25) * shadow;
                water += sun.color * pow(saturate(dot(r, sun.direction)), 350) * 2.5 * sun.shadowAttenuation;

                // Foam where the water thins out against the shore.
                float edge = 1 - saturate(depth / max(_FoamDepth, .01));
                float t = _Time.y * .35;
                float lace = NatureNoise(i.positionWS.xz * 2.6 + float2(t, -t * .7));
                float band = frac(edge * 2.2 - t * .6);
                float foam = smoothstep(.55, .62, edge * (.6 + lace * .7)) + smoothstep(.85, 1, edge) * .6;
                foam = saturate(foam + step(.9, band) * edge * lace * .8);
                water = lerp(water, _FoamColor.rgb * (half(0.8) + half(0.2) * shadow), foam * .85);

                float alpha = saturate(depth / .06);
                return half4(NatureFinite(MixFog(water, i.fog)), saturate(alpha));
            }
            ENDHLSL
        }
    }
}
