#ifndef CDP_LIGHTING_SHADERGRAPH_INCLUDED
#define CDP_LIGHTING_SHADERGRAPH_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

/*

struct SurfaceData
{
    half3 albedo;
    half3 specular;
    half  metallic;
    half  smoothness;
    half3 normalTS;
    half3 emission;
    half  occlusion;
    half  alpha;
    half  clearCoatMask;
    half  clearCoatSmoothness;
};

*/

void GetAllLights_half(
    half3 albedo,
    half3 specular,
    half  metallic,
    half  smoothness,
    half3 normalTS,
    half3 emission,
    half  occlusion,
    half  alpha,
    half  clearCoatMask,
    half  clearCoatSmoothness,
    out half3 diffuseOut,
    out half3 sepcularOut
)
{
    diffuseOut = 0;
    sepcularOut = 0;

    #if defined(DEBUG_DISPLAY)
    half4 debugColor;

    if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
    {
        return debugColor;
    }
    #endif

    uint meshRenderingLayers = GetMeshRenderingLayer();
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, aoFactor);

    inputData.bakedGI *= surfaceData.albedo;

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    ToonLighingResult toonRes;
    toonRes.color = 0;
    toonRes.specular = 0;

#ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
#endif
    {
        ToonLighingResult tr = CalculateToonLighting(mainLight, inputData, surfaceData);
        toonRes.color += tr.color;
        toonRes.specular += tr.specular;
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            ToonLighingResult tr = CalculateToonLighting(light, inputData, surfaceData);
            toonRes.color += tr.color;
            toonRes.specular += tr.specular;
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
#endif
        {
            ToonLighingResult tr = CalculateToonLighting(light, inputData, surfaceData);
            toonRes.color += tr.color;
            toonRes.specular += tr.specular;
        }
    LIGHT_LOOP_END
    #endif

    toonRes.color = floor(
        length(toonRes.color * steps / float3(
            surfaceData.albedo.x ? surfaceData.albedo.x : 1,
            surfaceData.albedo.y ? surfaceData.albedo.y : 1,
            surfaceData.albedo.z ? surfaceData.albedo.z : 1
        ))
    ) / steps * surfaceData.albedo;
    toonRes.specular = floor(length(toonRes.specular * steps)) / steps;

    lightingData.mainLightColor = toonRes.color * surfaceData.albedo + toonRes.specular;

    return half4(
        CalculateToonLightingColor(lightingData, 1),
        surfaceData.alpha
    );
}


#endif