// Procedural sunny Andean sky for Plaza Núñez: clear blue gradient, sun and halo, light drifting
// clouds and three mountain ranges (snowy far peaks, green mid and near ridges). Mountain faces
// are lit from the sun's side using the slope of each ridge, so they read as real relief.
// Everything is analytic, so it has no textures and works at any resolution.
Shader "Nemequene/Andean Sky"
{
    Properties
    {
        _ZenithColor ("Zenith", Color) = (0.20, 0.52, 0.90, 1)
        _SkyColor ("Upper sky", Color) = (0.46, 0.74, 0.98, 1)
        _HorizonColor ("Horizon", Color) = (0.84, 0.93, 1.00, 1)
        _FogColor ("Fog / below horizon", Color) = (0.74, 0.85, 0.93, 1)
        _SunColor ("Sun", Color) = (1, 0.97, 0.86, 1)
        _SunDirection ("Sun direction", Vector) = (0.4, 0.45, 0.8, 0)
        _SunSize ("Sun size", Range(0.995, 0.99995)) = 0.9992
        _CloudColor ("Clouds", Color) = (1, 1, 1, 1)
        _CloudCover ("Cloud cover", Range(0, 1)) = 0.38
        _CloudSpeed ("Cloud speed", Float) = 0.004
        _FarMountain ("Far range", Color) = (0.36, 0.45, 0.62, 1)
        _MidMountain ("Mid range", Color) = (0.26, 0.48, 0.30, 1)
        _NearMountain ("Near range", Color) = (0.22, 0.42, 0.24, 1)
        _Rock ("Rock", Color) = (0.50, 0.43, 0.37, 1)
        _Snow ("Snow", Color) = (0.97, 0.98, 1.0, 1)
        _MountainScale ("Mountain height", Range(0.2, 2.5)) = 1.7
        _Haze ("Distance haze", Range(0, 1)) = 0.18
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _ZenithColor, _SkyColor, _HorizonColor, _FogColor, _SunColor, _CloudColor;
            half4 _FarMountain, _MidMountain, _NearMountain, _Rock, _Snow;
            float4 _SunDirection;
            float _SunSize, _CloudCover, _CloudSpeed, _MountainScale, _Haze;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.direction = input.positionOS.xyz;
                return o;
            }

            float hash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p); float2 u = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), u.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), u.x), u.y);
            }
            float fbm(float2 p) { float v = 0, a = 0.5; for (int k = 0; k < 5; k++) { v += a * noise(p); p *= 2.03; a *= 0.5; } return v; }

            // Periodic ridge profile around the horizon (whole-cycle frequencies wrap at 2π). The
            // 1 - |sin| terms give sharp peaks and rounded valleys, like a real range.
            float ridge(float az, float seed, float roughness)
            {
                float h = 0;
                h += 0.46 * pow(1 - abs(sin(1.5 * az + seed)), 1.6);
                h += 0.30 * pow(1 - abs(sin(3.5 * az + seed * 1.7)), 2.0);
                h += 0.14 * (1 - abs(sin(8 * az + seed * 2.3))) * roughness;
                h += 0.07 * (1 - abs(sin(19 * az + seed * 3.1))) * roughness;
                h += 0.035 * sin(41 * az + seed * 4.9) * roughness;
                return h;
            }
            // Height and slope along the horizon (slope drives the sun-side lighting).
            void range(float az, float base, float amp, float seed, float roughness, out float h, out float slope)
            {
                const float e = 0.004;
                h = (base + amp * ridge(az, seed, roughness)) * _MountainScale;
                float h2 = (base + amp * ridge(az + e, seed, roughness)) * _MountainScale;
                slope = (h2 - h) / e;
            }
            // Faces rising towards the sun are lit, faces rising away are in shade.
            half3 shade(half3 albedo, float slope, float sunSide, float depth)
            {
                float lit = saturate(0.55 + slope * sunSide * 2.2);
                half3 c = albedo * lerp(0.5, 1.3, lit);
                return c * lerp(1.0, 0.85, depth);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 d = normalize(input.direction);
                float3 sun = normalize(_SunDirection.xyz);
                float elevation = d.y;
                float az = atan2(d.x, d.z);
                float sunAz = atan2(sun.x, sun.z);
                float sunDot = dot(d, sun);
                // +1 when the sun is to one side of this azimuth, -1 on the other.
                float sunSide = sin(sunAz - az);

                // Clear blue sky: pale near the horizon, deep blue overhead.
                float up = saturate(elevation);
                half3 sky = lerp(_SkyColor.rgb, _ZenithColor.rgb, pow(up, 0.55));
                sky = lerp(_HorizonColor.rgb, sky, saturate(pow(up * 4.0, 0.6)));
                sky += _SunColor.rgb * pow(saturate(sunDot), 8) * 0.25;

                // Light fair-weather clouds on a virtual dome.
                if (elevation > 0.0)
                {
                    float2 uv = d.xz / (elevation + 0.15) * 0.8 + _Time.y * _CloudSpeed * float2(1, 0.4);
                    float c = fbm(uv * 1.2);
                    float cloud = smoothstep(1 - _CloudCover, 1.08 - _CloudCover * 0.6, c) * saturate(elevation * 5);
                    half3 cloudTint = lerp(_CloudColor.rgb * 0.9, _CloudColor.rgb * 1.05, saturate(fbm(uv * 2.4 + 3)));
                    sky = lerp(sky, cloudTint, cloud * 0.8);
                }

                // Sun disc and glare.
                float disc = smoothstep(_SunSize, _SunSize + 0.0003, sunDot);
                sky = lerp(sky, _SunColor.rgb * 2.4, disc);
                sky += _SunColor.rgb * pow(saturate(sunDot), 250) * 0.8;

                half3 color = sky;
                float h, slope;
                // Far range: tall peaks with rock and broken snowcaps, bluish with distance.
                range(az, 0.05, 0.20, 0.7, 1.0, h, slope);
                if (elevation < h)
                {
                    float t = saturate((h - elevation) / max(h, 0.001));
                    // Rock strata and gullies: streaks that follow the slope down from the crest.
                    float strata = fbm(float2(az * 70 + elevation * 25 * sign(slope), elevation * 160));
                    half3 rock = lerp(_FarMountain.rgb, _Rock.rgb, 0.2 + 0.35 * strata);
                    half3 c = shade(rock, slope, sunSide, t) * lerp(0.85, 1.1, strata);
                    float snowLine = h - (0.03 + 0.03 * fbm(float2(az * 30, elevation * 60))) * _MountainScale;
                    float snow = smoothstep(snowLine - 0.004, snowLine + 0.003, elevation) * smoothstep(0.10, 0.16, h / _MountainScale);
                    half3 snowLit = _Snow.rgb * lerp(0.78, 1.12, saturate(0.55 + slope * sunSide * 2.2));
                    c = lerp(c, snowLit, snow);
                    // Sunlit rim along the crest outlines the peaks against the sky.
                    c = lerp(c, _Snow.rgb, smoothstep(0.012, 0.0, h - elevation) * 0.35);
                    color = lerp(c, _FogColor.rgb, _Haze * (0.55 + 0.35 * t));
                }
                // Mid range: green slopes.
                range(az + 1.3, 0.025, 0.11, 2.9, 0.8, h, slope);
                if (elevation < h)
                {
                    float t = saturate((h - elevation) / max(h, 0.001));
                    float patches = fbm(float2(az * 45, elevation * 70));
                    half3 grass = lerp(_MidMountain.rgb * 0.82, lerp(_MidMountain.rgb, _Rock.rgb, 0.25), patches);
                    color = lerp(shade(grass, slope, sunSide, t), _FogColor.rgb, _Haze * (0.3 + 0.35 * t));
                }
                // Near range: low foothills, darker and crisper.
                range(az - 0.6, 0.004, 0.05, 5.1, 0.6, h, slope);
                if (elevation < h)
                {
                    float t = saturate((h - elevation) / max(h, 0.001));
                    half3 hill = _NearMountain.rgb * lerp(0.85, 1.1, fbm(float2(az * 90, elevation * 140)));
                    color = lerp(shade(hill, slope, sunSide, t), _FogColor.rgb, _Haze * (0.15 + 0.4 * t));
                }
                // Below the horizon: a hazy green valley floor instead of empty fog.
                if (elevation < 0)
                {
                    half3 valley = lerp(_NearMountain.rgb * 0.9, _FogColor.rgb, 0.45 + 0.2 * fbm(float2(az * 20, elevation * 40)));
                    color = lerp(color, valley, saturate(-elevation * 25));
                }
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
