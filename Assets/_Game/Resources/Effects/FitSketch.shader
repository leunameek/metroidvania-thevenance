// The sketch of a piece in its table (FitPuzzle), part one: a faint glassy fill that brightens at
// the rims. It marks the stencil, so the line (Fit Sketch Line, drawn after it) only shows around
// it. Together they read as a drawing of where the piece goes, not as a second piece. _Glow
// (0..1) brightens it as the piece nears its pose.
Shader "Nemequene/Fit Sketch"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.82, 0.42, 1)
        _Fill ("Fill", Range(0, 1)) = 0.16
        _Rim ("Rim", Range(0, 2)) = 1
        _Outline ("Outline width (screen)", Float) = 0.006
        _Glow ("Glow", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+60" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        half4 _Color;
        float _Fill, _Rim, _Outline, _Glow;
        CBUFFER_END
        struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
        struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };
        ENDHLSL

        // The fill: faint in the middle, bright at the rims.
        Pass
        {
            Name "Fill"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
            Stencil { Ref 77 Comp Always Pass Replace }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float3 v = normalize(_WorldSpaceCameraPos - i.positionWS);
                float rim = pow(1 - saturate(abs(dot(normalize(i.normalWS), v))), 2);
                half3 c = lerp(_Color.rgb, half3(1, .98, .86), _Glow * .5) * (1 + rim + _Glow * .6);
                return half4(c, saturate((_Fill + _Glow * .12 + rim * _Rim * .6) * _Color.a));
            }
            ENDHLSL
        }
    }
}
