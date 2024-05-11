Shader "Unlit/ToonGeneral"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0,1)) = 0.5
        _Ratio ("Ratio", Range(0,1)) = 0.3
        _Strength ("Strength", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half3 worldNormal: NORMAL;
                float3 coords : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            
            float _Brightness;
            float _Ratio;
            float _Strength;

            float Toon(float3 normal, float3 lightDir)
            {
                
                float NdotL = max (0.0, dot (normalize(normal), normalize(lightDir)));

                return floor(NdotL/_Ratio);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.coords = v.vertex.xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                col *= Toon(i.worldNormal, _WorldSpaceLightPos0.xyz)*_Strength+_Brightness;
                
                return col;
            }
            ENDCG
        }
    }
}
