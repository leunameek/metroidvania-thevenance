// Shared pieces of the stylized nature shaders (ground, grass, foliage, water): value noise, the
// wind field and a soft painterly light (wrapped diffuse, coloured shade, real shadows and fog).
#ifndef NEMEQUENE_NATURE_COMMON
#define NEMEQUENE_NATURE_COMMON

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float NatureHash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
float NatureNoise(float2 p)
{
    float2 i = floor(p), f = frac(p); float2 u = f * f * (3 - 2 * f);
    return lerp(lerp(NatureHash(i), NatureHash(i + float2(1, 0)), u.x), lerp(NatureHash(i + float2(0, 1)), NatureHash(i + float2(1, 1)), u.x), u.y);
}
float NatureFbm(float2 p) { float v = 0, a = .5; for (int k = 0; k < 4; k++) { v += a * NatureNoise(p); p = p * 2.07 + 13.1; a *= .5; } return v; }

// A breeze that rolls across the field in gusts: x/z push in world space, 0..1 strength.
float3 NatureWind(float3 positionWS, float strength, float speed)
{
    float t = _Time.y * speed;
    float2 dir = normalize(float2(1, .45));
    float wave = sin(dot(positionWS.xz, dir) * .35 - t * 1.7) * .5 + .5;
    float gust = NatureNoise(positionWS.xz * .045 - dir * t * .55);
    float flutter = sin(t * 5.3 + positionWS.x * 1.7 + positionWS.z * 1.3) * .18;
    float push = (wave * .55 + gust * .9 + flutter) * strength;
    float2 side = float2(-dir.y, dir.x) * sin(t * 2.3 + positionWS.z) * .25 * strength;
    return float3(dir.x * push + side.x, 0, dir.y * push + side.y);
}

// Painterly light: wrapped sun, shade tinted towards the sky, ambient from the probes.
half3 NatureLight(half3 albedo, float3 positionWS, half3 normalWS, half wrap, half3 shadeTint)
{
    float4 shadowCoord = TransformWorldToShadowCoord(positionWS);
    Light sun = GetMainLight(shadowCoord);
    half ndl = dot(normalWS, sun.direction);
    half diffuse = saturate((ndl + wrap) / (1 + wrap));
    // The sun is directional: its distance attenuation (unity_LightData.z) is not filled in for
    // these runtime-built and instanced meshes, so it is left out.
    half shadow = half(1.0) - half(0.85) * (half(1.0) - sun.shadowAttenuation);
    half lit = diffuse * shadow;
    half3 ambient = SampleSH(normalWS);
    half3 direct = sun.color * lit;
    half3 shade = ambient * lerp(half3(1, 1, 1), shadeTint, 1 - lit);
    return albedo * (direct + shade);
}

// A colour the bloom can take: no NaN or infinity (some GPUs produce them from degenerate normals
// of imported meshes, and the bloom spreads one bad pixel into a white blob), and no runaway HDR.
half3 NatureFinite(half3 c)
{
    c = (c == c) ? c : half3(0, 0, 0);
    return clamp(c, half3(0, 0, 0), half3(8, 8, 8));
}

// Shadow caster position (the light direction is set by URP for the shadow pass).
float3 _LightDirection;
float3 _LightPosition;
float4 NatureShadowClip(float3 positionWS, float3 normalWS)
{
#if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
    float3 lightDirection = normalize(_LightPosition - positionWS);
#else
    float3 lightDirection = _LightDirection;
#endif
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirection));
#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif
    return positionCS;
}

// The normal for the depth-normals prepass (screen-space ambient occlusion reads it).
half4 NatureDepthNormal(float3 normalWS)
{
    float3 n = SafeNormalize(normalWS);
#if defined(_GBUFFER_NORMALS_OCT)
    float2 octahedral = PackNormalOctQuadEncode(n);
    float2 remapped = saturate(octahedral * .5 + .5);
    return half4(PackFloat2To888(remapped), 0);
#else
    return half4(n, 0);
#endif
}

#endif
