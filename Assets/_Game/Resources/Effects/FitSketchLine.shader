// The sketch of a piece in its table (FitPuzzle), part two: the bright line around the piece's
// own shape, drawn after the fill (Fit Sketch) and only where the fill is not.
Shader "Nemequene/Fit Sketch Line"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.82, 0.42, 1)
        _Fill ("Fill", Range(0, 1)) = 0.1
        _Rim ("Rim", Range(0, 2)) = 1
        _Outline ("Outline width (screen)", Float) = 0.006
        _Glow ("Glow", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+61" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
        half4 _Color;
        float _Fill, _Rim, _Outline, _Glow;
        CBUFFER_END
        struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
        struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };
        ENDHLSL

        // The outline: the back faces pushed out along the normals, by a width constant on screen,
        // drawn only around the fill (the stencil it left), so it reads as a line.
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha
            Stencil { Ref 77 Comp NotEqual }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            Varyings Vert(Attributes i)
            {
                Varyings o;
                float3 n = TransformObjectToWorldNormal(i.normalOS);
                float3 p = TransformObjectToWorld(i.positionOS.xyz);
                p += n * _Outline * distance(p, _WorldSpaceCameraPos);
                o.positionWS = p; o.normalWS = n;
                o.positionCS = TransformWorldToHClip(p);
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                half3 c = lerp(_Color.rgb, half3(1, .98, .86), _Glow * .6) * (1.3 + _Glow);
                return half4(c, _Color.a * (.85 + .15 * _Glow));
            }
            ENDHLSL
        }

    }
}
