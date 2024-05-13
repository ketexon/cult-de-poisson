#ifndef CDP_TOON_WATER_INPUT_INCLUDED
#define CDP_TOON_WATER_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;

    half _Cutoff;
    half _Surface;
	float4 _SurfaceNoise_ST;
	float4 _SurfaceDistortion_ST;

	float4 _DepthGradientShallow;
	float4 _DepthGradientDeep;
	float4 _FoamColor;

	float _DepthMaxDistance;
	float _FoamMaxDistance;
	float _FoamMinDistance;
	float _SurfaceNoiseCutoff;
	float _SurfaceDistortionAmount;

	float2 _SurfaceNoiseScroll;
CBUFFER_END

sampler2D _SurfaceDistortion;
sampler2D _SurfaceNoise;


#endif