Shader "Unlit/Triplanartest"
{

    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1) //HLSL requires defining inspector constants here
        _MainTex ("Texture", 2D) = "white" {}
        _SecTex ("Texture", 2D) = "white" {}
        _TerTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0,1)) = 0.5
        _Ratio ("Ratio", Range(0,1)) = 0.3
        _Strength ("Strength", Range(0,1)) = 0.2
        
        _Tiling ("Tiling", Float) = 1.0

        _Falloff ("Falloff", Range(0,1)) = 0.1
        // Control Texture ("Splat Map")
        [HideInInspector] _Control ("Control (RGBA)", 2D) = "red" {}
     
        // Terrain textures - each weighted according to the corresponding colour
        // channel in the control texture
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
    
    }
    SubShader
    {
        Tags {
            "SplatCount" = "4"
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
                // Access the Shaderlab properties
            uniform sampler2D _Control;
            uniform sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
            uniform fixed4 _Color;

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

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
                LIGHTING_COORDS(2,3)
            };

            float _Tiling;

            sampler2D _MainTex; //kinda like importing the variables defined at the very top
            sampler2D _SecTex;
            sampler2D _TerTex;
            float4 _MainTex_ST;
            float _Brightness;
            float _Ratio;
            float _Strength;
            
            float _Falloff;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.coords = v.vertex.xyz * _Tiling;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_VERTEX_TO_FRAGMENT(o); 
                return o;
            }


            float Toon(float3 normal, float3 lightDir)
            {                
                float NdotL = max (0.0, dot (normalize(normal), normalize(lightDir)));
                return floor(NdotL/_Ratio);
            }

            fixed4 SplitMap(fixed4 map){
                map.r = step(0.1, map.r - map.g - map.b - map.a);
                map.g = step(0.1, map.g - map.r - map.b - map.a);
                map.b = step(0.1, map.b - map.g - map.r - map.a);
                map.a = step(0.1, map.a - map.g - map.b - map.r);

                return map;
            }

            fixed4 SampleTexture(sampler2D tex, float3 coordinate){ //sampler2D is the texture image
                return (tex2D(tex, coordinate.yz) //sample in each plane axis?
                    + tex2D(tex, coordinate.xy)
                    + tex2D(tex, coordinate.xz)
                    )/3; //divides by 3 as average of the 3 samples. Can be done without xz axis
            }

            fixed4 SampleTextureFlat(sampler2D tex, float3 coordinate){
                return (tex2D(tex, coordinate.xz));
            };

            struct Input {
                float2 uv_Control : TEXCOORD0;
                float2 uv_Splat0 : TEXCOORD2;
                float2 uv_Splat1 : TEXCOORD3;
                float2 uv_Splat2 : TEXCOORD4;
                float2 uv_Splat3 : TEXCOORD5;
            };

            //frag shader
            fixed4 frag (v2f i, Input IN) : SV_Target
            {
                fixed atten = LIGHT_ATTENUATION(i);
                float steepness = dot(normalize(abs(i.worldNormal)), float3(0, 1, 0)); //get dot product of normal with up vector. can be returned as a shader
                float textureChoice = step(_Falloff, steepness); //strictly divides world into steep and not steep parts based on a Falloff value
                //steepness and textureChoice are both between 0 and 1

                fixed4 splat_control = SplitMap(tex2D (_Control, IN.uv_Control));

                //splat_control = SplitMap(splat_control);

                //sample the texture
                
                fixed4 col2 =
                    //textureChoice * SampleTextureFlat(_MainTex, i.coords)
                    + (1 - textureChoice) * SampleTexture(_SecTex, i.coords);

                fixed4 col = splat_control.r * tex2D (_Splat0, i.coords).rgba;
                col += splat_control.g * tex2D (_Splat1, i.coords).rgba;
                col += splat_control.b * tex2D (_Splat2, i.coords).rgba;
                col += splat_control.a * tex2D (_Splat3, i.coords).rgba;

                col -= (1 - textureChoice) * col;
                col += col2;
                col *= Toon(i.worldNormal, _WorldSpaceLightPos0.xyz)*_Strength+_Brightness;

                return col;
                
            }

            ENDCG
        }
    }

    Fallback "Standard"
}
