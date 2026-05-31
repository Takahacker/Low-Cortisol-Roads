Shader "Custom/GradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.45, 0.55, 0.75, 1)
        _MidColor ("Mid Color", Color) = (0.85, 0.65, 0.55, 1)
        _BotColor ("Bottom Color", Color) = (0.08, 0.10, 0.20, 1)
        _MidStart ("Mid Start", Range(0, 1)) = 0.3
        _MidEnd ("Mid End", Range(0, 1)) = 0.5
        _Blend ("Blend Smoothness", Range(0.01, 0.5)) = 0.15
        _SunColor ("Sun Color", Color) = (1, 0.85, 0.6, 1)
        _SunSize ("Sun Size", Range(0, 0.2)) = 0.06
        _SunX ("Sun X", Range(-1, 1)) = 0.3
        _SunY ("Sun Y", Range(-0.5, 1)) = 0.25
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _TopColor;
            float4 _MidColor;
            float4 _BotColor;
            float _MidStart;
            float _MidEnd;
            float _Blend;
            float4 _SunColor;
            float _SunSize;
            float _SunX;
            float _SunY;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; float3 viewDir : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.viewDir);
                float t = dir.y * 0.5 + 0.5;

                // Smooth transitions between 3 zones
                float botToMid = smoothstep(_MidStart - _Blend, _MidStart + _Blend, t);
                float midToTop = smoothstep(_MidEnd - _Blend, _MidEnd + _Blend, t);

                float4 col = lerp(_BotColor, _MidColor, botToMid);
                col = lerp(col, _TopColor, midToTop);

                // Sun
                float sunDist = length(float2(dir.x - _SunX, dir.y - _SunY));
                float sun = smoothstep(_SunSize, _SunSize * 0.3, sunDist);
                float glow = smoothstep(_SunSize * 5.0, _SunSize * 0.5, sunDist) * 0.3;
                col.rgb += _SunColor.rgb * (sun + glow);

                return col;
            }
            ENDCG
        }
    }
}
