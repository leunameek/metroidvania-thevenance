Shader "Nemequene/UI/Living Lagoon"
{
    Properties
    {
        [PerRendererData] _MainTex ("Painting", 2D) = "white" {}
        _AmbientTime ("Ambient time", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            sampler2D _MainTex;
            float _AmbientTime;
            v2f vert(appdata v)
            {
                v2f o; o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;
            }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p)
            {
                float2 cell=floor(p),f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(cell),hash(cell+float2(1,0)),f.x),
                    lerp(hash(cell+float2(0,1)),hash(cell+1),f.x),f.y);
            }
            fixed4 frag(v2f i):SV_Target
            {
                float t=_AmbientTime;
                float2 uv=i.uv;
                // Two open channels; the figure, buildings and trees remain still.
                float nearWater=smoothstep(.18,.22,uv.y)*(1-smoothstep(.29,.32,uv.y))
                    *smoothstep(.33,.43,uv.x)*(1-smoothstep(.64,.73,uv.x));
                float farWater=smoothstep(.44,.46,uv.y)*(1-smoothstep(.49,.515,uv.y))
                    *smoothstep(.69,.77,uv.x)*(1-smoothstep(.95,1,uv.x));
                float water=max(nearWater,farWater);
                float ripple=sin(uv.y*390+t*.8+sin(uv.x*24+t*.23));
                float2 sampleUV=uv;
                sampleUV.x+=water*ripple*.00085;
                fixed4 col=tex2D(_MainTex,sampleUV);
                // Slow, translucent mist travels through the middle-distance valley.
                float valley=smoothstep(.47,.53,uv.y)*(1-smoothstep(.62,.68,uv.y))
                    *smoothstep(.23,.38,uv.x)*(1-smoothstep(.92,1,uv.x));
                float2 wind=float2(t*.013,t*.002);
                float mist=noise(uv*float2(6,17)-wind)*.66
                    +noise(uv*float2(15,36)-wind*1.5)*.34;
                float veil=smoothstep(.36,.83,mist)*valley*.095;
                col.rgb=lerp(col.rgb,fixed3(.70,.67,.59),veil);
                return col*i.color;
            }
            ENDCG
        }
    }
}
