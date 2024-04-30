#ifndef CDP_TOON_WATER_FORWARD_PASS_INCLUDED
#define CDP_TOON_WATER_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

// Blends two colors using the same algorithm that our shader is using
// to blend with the screen. This is usually called "normal blending",
// and is similar to how software like Photoshop blends two layers.
float4 alphaBlend(float4 top, float4 bottom)
{
	float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
	float alpha = top.a + bottom.a * (1 - top.a);

	return float4(color, alpha);
}

struct Attributes
{
	float4 positionOS 	: POSITION;
	float3 normalOS 	: NORMAL;

	float2 texcoord 	: TEXCOORD0;
};

struct Varyings
{
	float2 noiseUV : TEXCOORD0;
	float2 distortUV : TEXCOORD1;

	float3 positionWS : TEXCOORD2;
	float3 positionVS : TEXCOORD5;
	half3 normalWS : TEXCOORD3;

	half fogFactor : TEXCOORD4;

	float4 positionCS : SV_POSITION;
};

Varyings ToonWaterVertex(Attributes input)
{
	Varyings output;

	VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
	VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

	#if defined(_FOG_FRAGMENT)
        half fogFactor = 0;
	#else
        half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
	#endif

	output.positionWS.xyz = vertexInput.positionWS;
	output.positionVS = vertexInput.positionVS;
	output.positionCS = vertexInput.positionCS;
	output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);

	output.fogFactor = fogFactor;

	output.distortUV = TRANSFORM_TEX(input.texcoord, _SurfaceDistortion);
	output.noiseUV = TRANSFORM_TEX(input.texcoord, _SurfaceNoise);

	return output;
}

void ToonWaterFragment(
	Varyings input,
	out half4 outColor : SV_Target0
)
{
	// // Retrieve the current depth value of the surface behind the
	// // pixel we are currently rendering.
	float2 screenSpaceUV = input.positionCS.xy / _ScaledScreenParams.xy;
#if UNITY_REVERSED_Z
	float depth = SampleSceneDepth(screenSpaceUV);
#else
    // Adjust z to match NDC for OpenGL
    float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenSpaceUV));
#endif
	float3 seabedPositionWS = ComputeWorldSpacePosition(screenSpaceUV, depth, UNITY_MATRIX_I_VP);
	// float x = input.positionVS.x - 10;
	// float y = input.positionVS.y;
	// float z = input.positionCS.z;
	// existingDepth01 = z;
	// existingDepth01 = input.positionCS.z;
	// // Convert the depth from non-linear 0...1 range to linear
	// // depth, in Unity units.
	// float existingDepthLinear = LinearEyeDepth(existingDepth01);

	// // Difference, in Unity units, between the water's surface and the object behind it.
	float depthDifference = input.positionCS.z - depth;

	// // Calculate the color of the water based on the depth using our two gradient colors.
	float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
	// float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);
	float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);

	// Retrieve the view-space normal of the surface behind the
	// pixel we are currently rendering.
	float3 existingNormal = SampleSceneNormals(screenSpaceUV);

	// Modulate the amount of foam we display based on the difference
	// between the normals of our water surface and the object behind it.
	// Larger differences allow for extra foam to attempt to keep the overall
	// amount consistent.

	float distanceFromBottom = length(input.positionWS - seabedPositionWS);
	float foamVisibility = 1 - (distanceFromBottom - _FoamMinDistance)/(_FoamMaxDistance - _FoamMinDistance);
	foamVisibility = clamp(foamVisibility, 0, 1);

	float surfaceNoiseCutoff = _SurfaceNoiseCutoff / foamVisibility;
	float2 distortSample = (tex2D(_SurfaceDistortion, input.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;

	// Distort the noise UV based off the RG channels (using xy here) of the distortion texture.
	// Also offset it by time, scaled by the scroll speed.
	float2 noiseUV = float2(
		(input.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x,
		(input.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y
	);
	float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

	// Use smoothstep to ensure we get some anti-aliasing in the transition from foam to surface.
	// Uncomment the line below to see how it looks without AA.
	// float surfaceNoise = surfaceNoiseSample > surfaceNoiseCutoff ? 1 : 0;
	float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);

	float4 surfaceNoiseColor = _FoamColor;
	surfaceNoiseColor.a *= surfaceNoise;
	surfaceNoiseColor.a *= foamVisibility;

	// Use normal alpha blending to combine the foam with the surface.
	half4 color = alphaBlend(surfaceNoiseColor, waterColor);

	half fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);

	color.rgb = MixFog(color.rgb, fogCoord);

	outColor = color;
	// float test = dot(existingNormal, input.normalWS);
	float test = distanceFromBottom / 10;
	// outColor = half4(test,test,test,1);
}

#endif