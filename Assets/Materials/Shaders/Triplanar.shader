Shader "Unlit/Triplanar"
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

        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
        
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
                return o;
            }


            float Toon(float3 normal, float3 lightDir)
            {
                
                float NdotL = max (0.0, dot (normalize(normal), normalize(lightDir)));
                return floor(NdotL/_Ratio);
            }

            fixed4 SampleTexture(sampler2D tex, float3 coordinate){ //sampler2D is the texture image
                return (tex2D(tex, coordinate.yz) //sample in each plane axis?
                    + tex2D(tex, coordinate.xy)
                    + tex2D(tex, coordinate.xz)
                    )/3; //divides by 3 as average of the 3 samples. Can be done without xz axis
            }

            fixed4 SampleTextureFlat(sampler2D tex, float3 coordinate){
                return (tex2D(tex, coordinate.xz));
            }

            //frag shader
            fixed4 frag (v2f i) : SV_Target
            {
                float steepness = dot(normalize(abs(i.worldNormal)), float3(0, 1, 0)); //get dot product of normal with up vector. can be returned as a shader
                float textureChoice = step(_Falloff, steepness); //strictly divides world into steep and not steep parts based on a Falloff value
                //steepness and textureChoice are both between 0 and 1

                //sample the texture
                fixed4 col = 
                    textureChoice * SampleTextureFlat(_MainTex, i.coords)
                    + (1 - textureChoice) * SampleTexture(_SecTex, i.coords);

                col *= Toon(i.worldNormal, _WorldSpaceLightPos0.xyz)*_Strength+_Brightness;

                return col;

                // --> comment statement
                /* --> actual old code */

                //Blending factor, not sure exactly what it does though
                /* float3 bf = abs(i.worldNormal);
                   bf /= dot(bf, 1); */

                // Base color
                
                /*
                fixed4 cx = tex2D(_SecTex, i.coords.yz) * bf.x; //Steeper surfaces
                fixed4 cy = tex2D(_MainTex, i.coords.xz) * bf.y ; //Handles all flat surfaces
                fixed4 cz = step(0, tex2D(_SecTex, i.coords.xy) * bf.z) * tex2D(_SecTex, i.coords.xy) * bf.z; //Steeper surfaces
                */                
                

                // sample the texture

                /*
                
                fixed4 col2 = cx + cz + cy;
                col2 = col/2 + fixed4(1, 1, 1, 1)/2;
                
                col2 *= Toon(i.worldNormal, _WorldSpaceLightPos0.xyz)*_Strength+_Brightness;
                 
                return col2;
                
                */
                
                
                               
                //col.r = step(0.1, col.r - col.g - col.b - col.a);
	            //col.g = step(0.1, col.g - col.r - col.b - col.a);
	            //col.b = step(0.1, col.b - col.g - col.r - col.a);
	            //col.a = step(0.1, col.a - col.g - col.b - col.r);

                
	            
                
            }

            ENDCG
        }
    }

        Fallback "Standard"
}
