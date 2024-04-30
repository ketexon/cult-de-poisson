#ifndef CDP_LIGHTING_INCLUDED
#define CDP_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct ToonLighingResult {
    half3 color;
    half3 specular;
};

half3 CalculateToonLightingColor(LightingData lightingData, half3 albedo)
{
    half3 lightingColor = 0;

    if (IsOnlyAOLightingFeatureEnabled())
    {
        return lightingData.giColor; // Contains white + AO
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_GLOBAL_ILLUMINATION))
    {
        lightingColor += lightingData.giColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_MAIN_LIGHT))
    {
        lightingColor += lightingData.mainLightColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_ADDITIONAL_LIGHTS))
    {
        lightingColor += lightingData.additionalLightsColor;
    }

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_VERTEX_LIGHTING))
    {
        lightingColor += lightingData.vertexLightingColor;
    }

    lightingColor *= albedo;

    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_EMISSION))
    {
        lightingColor += lightingData.emissionColor;
    }

    return lightingColor;
}


ToonLighingResult CalculateToonLighting(Light light, InputData inputData, SurfaceData surfaceData)
{
    ToonLighingResult res;

    // half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
    half3 attenuatedLightColor = light.color;
    half3 lightDiffuseColor = LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);

    half3 lightSpecularColor = half3(0,0,0);

    #if defined(_SPECGLOSSMAP) || defined(_SPECULAR_COLOR)
    half smoothness = exp2(10 * surfaceData.smoothness + 1);
    lightSpecularColor += LightingSpecular(attenuatedLightColor, light.direction, inputData.normalWS, inputData.viewDirectionWS, half4(surfaceData.specular, 1), smoothness);
    #endif

    res.specular = lightSpecularColor;
#if _ALPHAPREMULTIPLY_ON
    res.color = lightDiffuseColor * surfaceData.albedo * surfaceData.alpha;
#else
    res.color = lightDiffuseColor * surfaceData.albedo;
#endif
    return res;
}

half4 ToonLighting(InputData inputData, SurfaceData surfaceData, int steps)
{
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

    toonRes.color = floor(length(toonRes.color * steps / surfaceData.albedo)) / steps * surfaceData.albedo;
    toonRes.specular = floor(length(toonRes.specular * steps)) / steps;

    lightingData.mainLightColor = toonRes.color * surfaceData.albedo + toonRes.specular;

    return half4(
        CalculateToonLightingColor(lightingData, 1),
        surfaceData.alpha
    );
}

#endif