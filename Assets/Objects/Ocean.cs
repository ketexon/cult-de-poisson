using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
    [SerializeField] int overrideSeed = 1;
    [SerializeField] int nSamples = 256;
    [SerializeField] float gravity = 100;
    [SerializeField] float physicalLength = 10.0f;
    [SerializeField] float phillipsA = 1.0f;
    [SerializeField] float windAngle = 0;
    [SerializeField] float windSpeed = 9.81f;
    [SerializeField] float period = 120.0f;

    [SerializeField] ComputeShader computeShader;

    RenderTexture initialHeightSpectrum = null;
    RenderTexture heightSpectrum = null;
    RenderTexture heightMap = null;

    Material mat;

    int nThreadGroupsX, nThreadGroupsY;

    static class Kernel {
        public const int InitializeHeightSpectrum = 0;
        public const int InitializeHeightSpectrumConjugates = 1;
        public const int UpdateHeightSpectrum = 2;
        public const int FFTHorizontal = 3;
        public const int FFTVertical = 4;
        public const int DFT = 5;
    }

    void Awake()
    {
        var renderer = GetComponent<Renderer>();
        mat = renderer.material;

        SetUniforms();

        nThreadGroupsX = Mathf.CeilToInt(nSamples / 8);
        nThreadGroupsY = Mathf.CeilToInt(nSamples / 8);

        initialHeightSpectrum = CreateRenderTexture();
        heightSpectrum = CreateRenderTexture();
        heightMap = CreateRenderTexture();

        GenerateInitialHeightSpectrum();
    }

    RenderTexture CreateRenderTexture()
    {
        var rt = new RenderTexture(nSamples, nSamples, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.enableRandomWrite = true;
        rt.useMipMap = false;
        rt.autoGenerateMips = false;
        rt.anisoLevel = 16;
        rt.Create();

        return rt;
    }

    void SetUniforms()
    {
        computeShader.SetFloat("_Seed", overrideSeed != 0 ? overrideSeed : Random.Range(0, int.MaxValue - 1));
        computeShader.SetFloat("_NSamples", nSamples);
        computeShader.SetFloat("_PhysicalLength", physicalLength);
        computeShader.SetFloat("_Gravity", gravity);
        computeShader.SetFloat("_Period", period);
        computeShader.SetFloat("_WindAngle", windAngle);
        computeShader.SetFloat("_WindSpeed", windSpeed);
        computeShader.SetFloat("_PhillipsA", phillipsA);
    }

    void GenerateInitialHeightSpectrum()
    {
        computeShader.SetTexture(
            Kernel.InitializeHeightSpectrum, 
            "_InitialHeightSpectrum", 
            initialHeightSpectrum
        );
        /// Generate initial spectrum h^tilde_0(k)
        computeShader.Dispatch(
            Kernel.InitializeHeightSpectrum,
            nThreadGroupsX, nThreadGroupsY, 1
        );


        computeShader.SetTexture(
            Kernel.InitializeHeightSpectrumConjugates, 
            "_InitialHeightSpectrum", 
            initialHeightSpectrum
        );
        /// Cache h^tilde_0(-k) with h^tilde_0(k)
        computeShader.Dispatch(
            Kernel.InitializeHeightSpectrumConjugates, 
            nThreadGroupsX, nThreadGroupsY, 1
        );
    }

    void UpdateHeightSpectrum()
    {
        computeShader.SetTexture(
            Kernel.UpdateHeightSpectrum, 
            "_InitialHeightSpectrum", 
            initialHeightSpectrum
        );
        computeShader.SetTexture(
            Kernel.UpdateHeightSpectrum,
            "_HeightSpectrum",
            heightSpectrum
        );
        computeShader.SetFloat(
            "_Time",
            Time.time
        );
        computeShader.Dispatch(Kernel.UpdateHeightSpectrum, nThreadGroupsX, nThreadGroupsY, 1);

        computeShader.SetTexture(
            Kernel.DFT,
            "_FTTarget",
            heightMap
        );
        computeShader.SetTexture(
            Kernel.DFT,
            "_HeightSpectrum",
            heightSpectrum
        );
        computeShader.Dispatch(
            Kernel.DFT,
            1, nSamples, 1
        );

        mat.SetTexture("_HeightMap", heightMap);

        RenderTexture.active = heightMap;

        Texture2D tex = new(heightMap.width, heightMap.height);
        tex.ReadPixels(new Rect(0, 0, heightMap.width, heightMap.height), 0, 0);

        Debug.Log(tex.GetPixel(120, 120));

        RenderTexture.active = null;

        //computeShader.SetTexture(
        //    Kernel.FFTHorizontal,
        //    "_FTTarget",
        //    heightSpectrum
        //);
        //computeShader.Dispatch(
        //    Kernel.FFTHorizontal,
        //    1, nSamples, 1
        //);

        //computeShader.SetTexture(
        //    Kernel.FFTVertical,
        //    "_FTTarget",
        //    heightSpectrum
        //);
        //computeShader.Dispatch(
        //    Kernel.FFTVertical,
        //    1, nSamples, 1
        //);
    }

    void Update()
    {
        UpdateHeightSpectrum();
        if(Mathf.FloorToInt(Time.time * 2) % 2 == 0)
        {
            mat.SetTexture("_MainTex", initialHeightSpectrum);
        }
        else
        {
            mat.SetTexture("_MainTex", heightSpectrum);
        }
    }

    void OnDestroy()
    {
        if(initialHeightSpectrum != null)
        {
            Destroy(initialHeightSpectrum);
        }
        if(heightSpectrum != null)
        {
            Destroy(heightSpectrum);
        }
        if (heightMap != null)
        {
            Destroy(heightMap);
        }
    }
}
