using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class CameraPixellater : MonoBehaviour
{
    [SerializeField] new Camera camera;
    [SerializeField] RawImage image;
    [SerializeField] int minWidth = 640;
    [SerializeField] int minHeight = 480;

    float MinAspect => (float) minWidth / minHeight;

    RenderTexture tex;

    UniversalAdditionalCameraData urpCamera;

    float lastAspect;

    void Start()
    {
        RecreateRenderTexture();
    }

    void OnDestroy()
    {
        if (tex)
        {
            tex.Release();
        }
    }

    void Update()
    {
        var screenAspect = (float)Screen.width / Screen.height;
        if(!Mathf.Approximately(screenAspect, lastAspect))
        {
            RecreateRenderTexture();
        }
    }

    void RecreateRenderTexture()
    {
        var screenAspect = (float) Screen.width / Screen.height;
        int actualWidth;
        int actualHeight;
        if (screenAspect * minHeight > minWidth)
        {
            actualHeight = minHeight;
            actualWidth = (int) (screenAspect * minHeight);
        }
        else
        {
            actualHeight = (int)(minWidth / screenAspect);
            actualWidth = minWidth;
        }

        if (tex)
        {
            tex.Release();
        }

        tex = new(actualWidth, actualHeight, 16, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat);
        tex.Create();

        lastAspect = screenAspect;

        camera.targetTexture = tex;
        image.texture = tex;
    }
}
