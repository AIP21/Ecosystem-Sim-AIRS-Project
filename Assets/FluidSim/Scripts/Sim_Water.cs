using System;
using System.Collections;
using UnityEngine;

public class Sim_Water : MonoBehaviour
{
    #region Public

    #region RenderTextures
    [Header("Render Textures")]
    // Surface Water
    public RenderTexture result;
    public RenderTexture surfaceWaterMap;
    public RenderTexture newSurfaceWaterMap;
    public RenderTexture surfaceFlowMap;
    public RenderTexture newSurfaceFlowMap;
    public RenderTexture surfaceVelocityMap;
    public RenderTexture newSurfaceVelocityMap;

    // Soil Water
    [Space(10)]
    public RenderTexture soilSaturationMap;
    public RenderTexture newSoilSaturationMap;
    public RenderTexture soilFlowMap;
    public RenderTexture newSoilFlowMap;

    [Space(10)]
    [Range(0, 5)]
    public int textureToDraw = 0;
    #endregion

    #region General Settings
    [Header("General Settings")]
    public Texture2D heightmap;

    [Space(10)]
    public int resolution = 1024;
    public float timeStep = 0.1f;
    public float epsilon = 1e-5f;
    #endregion

    #region Surface Water Settings
    [Header("Surface Water Settings")]
    public ComputeShader surfaceComputeShader;

    [Space(10)]
    public float waterDensity = 1.0f;
    public float gravitationAcceleration = 9.81f;
    public float cellHeight = 1;
    public float cellArea = 1; // should be the cell height squared
    public float diffuseAlpha = 1.0f;
    public float evaporationConstant = 0.01f;
    [Range(-20, 20)]
    public float heightmapMultiplier = 1.0f;
    public float flowDamping = 0.0f;

    [Space(10)]
    public bool enableInput = true;
    public bool enableFlow = true;
    public bool enableHeight = true;
    public bool enableVelocity = true;
    public bool enableVelocityDiffusion = true;
    #endregion

    #region Surface Water Input
    [Header("Surface Water source")]
    public bool sourceIsMouse = false;
    [Range(0, 1)]
    public float sourceRadius = 0.008f;
    public float sourceAmount = 1;
    public Vector2 sourcePosition;
    #endregion

    #region Soil Water Settings
    [Header("Soil Water Settings")]
    public ComputeShader soilComputeShader;
    public Texture2D soilMap; // water capacity (affects absorption rate (absorptivity), evaporation rate, flow rate, and max holding capacity)
    public Texture2D soilUseMap; // where water is being consumed (by trees, plants, etc.)

    [Space(10)]
    public float soilGravitationAcceleration = 9.81f;
    public float soilCellHeight = 1;
    public float soilCellArea = 1; // should be the soil cell height squared
    [Range(-20, 20)]
    public float soilHeightmapMultiplier = 1.0f;
    public float soilUseStrength = 1.0f;

    [Space(10)]
    public bool soilEnableUse = true;
    public bool soilEnableAbsorption = true;
    public bool soilEnableRelease = true;
    public bool soilEnableEvaporation = true;
    public bool soilEnableFlow = true;
    public bool soilEnableSaturation = true;
    #endregion

    [Space(25)]
    public bool showDebugTextures = true;
    #endregion

    #region Private
    private int dispatchSize = 0;
    private int surfaceKernelCount = 0;
    private int kernel_reset = 0;
    private int kernel_input = 0;
    private int kernel_evaporation = 0;
    private int kernel_slipPass = 0;
    private int kernel_flow = 0;
    private int kernel_height = 0;
    private int kernel_velocity = 0;
    private int kernel_diffusion = 0;

    private int soilKernelCount = 0;
    private int kernel_soilReset = 0;
    private int kernel_soilFlux = 0; // Use by plants, release to surface, absorption from surface, and evaporation
    private int kernel_soilFlow = 0;
    private int kernel_soilSlipPass = 0;
    private int kernel_soilSaturation = 0;
    #endregion

    private RenderTexture CreateTexture(RenderTextureFormat format, FilterMode filterMode = FilterMode.Point)
    {
        RenderTexture dataTex = new RenderTexture(resolution, resolution, 24, format);
        dataTex.filterMode = filterMode;
        dataTex.wrapMode = TextureWrapMode.Clamp;
        dataTex.enableRandomWrite = true;
        dataTex.Create();

        return dataTex;
    }

    private void DispatchCompute(ComputeShader shader, int kernel)
    {
        shader.Dispatch(kernel, dispatchSize, dispatchSize, 1);
    }

    private void Awake()
    {
        sourcePosition = new Vector2(0.5f, 0.5f);

        // Create render textures
        result = CreateTexture(RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);
        surfaceWaterMap = CreateTexture(RenderTextureFormat.RFloat);
        newSurfaceWaterMap = CreateTexture(RenderTextureFormat.RFloat);
        surfaceFlowMap = CreateTexture(RenderTextureFormat.ARGBHalf);
        newSurfaceFlowMap = CreateTexture(RenderTextureFormat.ARGBHalf);
        surfaceVelocityMap = CreateTexture(RenderTextureFormat.RGHalf, FilterMode.Bilinear);
        newSurfaceVelocityMap = CreateTexture(RenderTextureFormat.RGHalf, FilterMode.Bilinear);

        soilSaturationMap = CreateTexture(RenderTextureFormat.RFloat);
        newSoilSaturationMap = CreateTexture(RenderTextureFormat.RFloat);
        soilFlowMap = CreateTexture(RenderTextureFormat.ARGBHalf);
        newSoilFlowMap = CreateTexture(RenderTextureFormat.ARGBHalf);

        // Set shader variables
        surfaceComputeShader.SetFloat("waterDensity", waterDensity);
        surfaceComputeShader.SetFloat("gravityAcceleration", gravitationAcceleration);
        surfaceComputeShader.SetFloat("pipeLength", cellHeight);
        surfaceComputeShader.SetFloat("pipeArea", cellArea);
        surfaceComputeShader.SetFloat("epsilon", epsilon);
        surfaceComputeShader.SetFloat("diffuseAlpha", diffuseAlpha);
        surfaceComputeShader.SetVector("inputPosition", sourcePosition);
        surfaceComputeShader.SetFloat("inputRadius", sourceRadius);
        surfaceComputeShader.SetFloat("inputAmount", sourceAmount);
        surfaceComputeShader.SetFloat("flowDamping", 1.0f - flowDamping);
        surfaceComputeShader.SetFloat("_deltaTime", timeStep);
        surfaceComputeShader.SetFloat("heightmapMultiplier", heightmapMultiplier);
        surfaceComputeShader.SetFloat("evaporationConstant", evaporationConstant);
        surfaceComputeShader.SetFloat("size", resolution);

        // Find shader kernels
        kernel_reset = surfaceComputeShader.FindKernel("reset"); surfaceKernelCount++;
        kernel_input = surfaceComputeShader.FindKernel("computeInput"); surfaceKernelCount++;
        kernel_evaporation = surfaceComputeShader.FindKernel("computeEvaporation"); surfaceKernelCount++;
        kernel_flow = surfaceComputeShader.FindKernel("computeWaterFlow"); surfaceKernelCount++;
        kernel_height = surfaceComputeShader.FindKernel("computeWaterHeight"); surfaceKernelCount++;
        kernel_slipPass = surfaceComputeShader.FindKernel("applyFreeSlip"); surfaceKernelCount++;
        kernel_velocity = surfaceComputeShader.FindKernel("computeWaterVelocity"); surfaceKernelCount++;
        kernel_diffusion = surfaceComputeShader.FindKernel("computeDiffusedWaterVelocity"); surfaceKernelCount++;

        kernel_soilReset = soilComputeShader.FindKernel("reset"); soilKernelCount++;
        kernel_soilAbsorption = soilComputeShader.FindKernel("computeAbsorption"); soilKernelCount++;
        kernel_soilUse = soilComputeShader.FindKernel("computeUse"); soilKernelCount++;
        kernel_soilRelease = soilComputeShader.FindKernel("computeRelease"); soilKernelCount++;
        kernel_soilEvaporation = soilComputeShader.FindKernel("computeEvaporation"); soilKernelCount++;
        kernel_soilFreeSlip = soilComputeShader.FindKernel("applyFreeSlip"); soilKernelCount++;
        kernel_soilFlow = soilComputeShader.FindKernel("computeFlow"); soilKernelCount++;
        kernel_soilSaturation = soilComputeShader.FindKernel("computeSaturation"); soilKernelCount++;

        // Setup shader render textures
        // updateTextures();

        // Initialize shader textures
        dispatchSize = Mathf.CeilToInt(resolution / 8);
        DispatchCompute(surfaceComputeShader, kernel_reset);
        DispatchCompute(soilComputeShader, kernel_soilReset);
    }

    private void updateTextures()
    {
        for (int kernel = 0; kernel < kernelCount; kernel++)
        {
            /* 
			TODO: OPTIMIZE
            This example is not optimized, not all kernels read/write into all textures,
			but I keep it like this for the sake of convenience
			*/
            surfaceComputeShader.SetTexture(kernel, "heightmap", heightmap);
            surfaceComputeShader.SetTexture(kernel, "result", result);
            surfaceComputeShader.SetTexture(kernel, "waterMap", surfaceWaterMap);
            surfaceComputeShader.SetTexture(kernel, "newWaterMap", newSurfaceWaterMap);
            surfaceComputeShader.SetTexture(kernel, "flowMap", surfaceFlowMap);
            surfaceComputeShader.SetTexture(kernel, "newFlowMap", newSurfaceFlowMap);
            surfaceComputeShader.SetTexture(kernel, "velocityMap", surfaceVelocityMap);
            surfaceComputeShader.SetTexture(kernel, "newVelocityMap", newSurfaceVelocityMap);
        }
    }

    public void Update()
    {
        // Set shader variables
        surfaceComputeShader.SetFloat("waterDensity", waterDensity);
        surfaceComputeShader.SetFloat("epsilon", epsilon);
        surfaceComputeShader.SetFloat("diffuseAlpha", diffuseAlpha);
        surfaceComputeShader.SetFloat("_deltaTime", timeStep);
        surfaceComputeShader.SetFloat("heightmapMultiplier", heightmapMultiplier);

        // soilComputeShader.SetFloat();

        if (enableInput)
            surfaceInput();

        soilWaterFlux();

        applyFreeSlip();

        surfaceWaterFlow();

        if (soilEnableFlow)
            soilWaterFlow();

        if (enableVelocity)
            surfaceWaterVelocity();
    }

    private void surfaceInput()
    {
        if (sourceIsMouse)
        {
            // Add water at the mouse position while clicking
            surfaceComputeShader.SetVector("inputPosition", new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.width));
            surfaceComputeShader.SetFloat("inputRadius", sourceRadius);
            surfaceComputeShader.SetFloat("inputAmount", Input.GetMouseButton(0) ? sourceAmount : 0);
        }
        else
        {
            // Add water at the source position
            surfaceComputeShader.SetVector("inputPosition", sourcePosition);
            surfaceComputeShader.SetFloat("inputRadius", sourceRadius);
            surfaceComputeShader.SetFloat("inputAmount", sourceAmount);
        }

        surfaceComputeShader.SetTexture(kernel_input, "waterMap", surfaceWaterMap);
        surfaceComputeShader.SetTexture(kernel_input, "newWaterMap", newSurfaceWaterMap);

        DispatchCompute(kernel_input);

        Graphics.Blit(newSurfaceWaterMap, surfaceWaterMap);

        if (evaporationConstant > 0.0f)
        {
            surfaceComputeShader.SetFloat("evaporationConstant", evaporationConstant);

            surfaceComputeShader.SetTexture(kernel_evaporation, "waterMap", surfaceWaterMap);
            surfaceComputeShader.SetTexture(kernel_evaporation, "newWaterMap", newSurfaceWaterMap);

            DispatchCompute(kernel_evaporation);

            Graphics.Blit(newSurfaceWaterMap, surfaceWaterMap);
        }
    }

    private void soilWaterFlux()
    {
        if (soilEnableAbsorption)
        {
            // soilWaterComputeShader.SetFloat();

        }

        if (soilEnableUse)
        {

        }

        if (soilEnableRelease)
        {

        }

        if (soilEnableEvaporation)
        {

        }
    }

    private void applyFreeSlip()
    {
        // This function prevents the water from freaking out when it hits a border
        surfaceComputeShader.SetTexture(kernel_slipPass, "waterMap", surfaceWaterMap);
        surfaceComputeShader.SetTexture(kernel_slipPass, "newWaterMap", newSurfaceWaterMap);

        DispatchCompute(kernel_slipPass);
        Graphics.Blit(newSurfaceWaterMap, surfaceWaterMap);
    }

    private void surfaceWaterFlow()
    {
        if (enableFlow)
        {
            surfaceComputeShader.SetFloat("gravityAcceleration", gravitationAcceleration);
            surfaceComputeShader.SetFloat("pipeLength", cellHeight);
            surfaceComputeShader.SetFloat("pipeArea", cellArea);
            surfaceComputeShader.SetFloat("flowDamping", 1.0f - flowDamping);

            surfaceComputeShader.SetTexture(kernel_flow, "heightmap", heightmap);
            surfaceComputeShader.SetTexture(kernel_flow, "waterMap", surfaceWaterMap);
            surfaceComputeShader.SetTexture(kernel_flow, "flowMap", surfaceFlowMap);
            surfaceComputeShader.SetTexture(kernel_flow, "newFlowMap", newSurfaceFlowMap);

            DispatchCompute(kernel_flow);

            Graphics.Blit(newSurfaceFlowMap, surfaceFlowMap);
        }

        if (enableHeight)
        {
            surfaceComputeShader.SetFloat("pipeLength", cellHeight);

            surfaceComputeShader.SetTexture(kernel_height, "heightmap", heightmap);
            surfaceComputeShader.SetTexture(kernel_height, "flowMap", surfaceFlowMap);
            surfaceComputeShader.SetTexture(kernel_height, "waterMap", surfaceWaterMap);
            surfaceComputeShader.SetTexture(kernel_height, "newWaterMap", newSurfaceWaterMap);
            surfaceComputeShader.SetTexture(kernel_height, "result", result);

            DispatchCompute(kernel_height);

            Graphics.Blit(newSurfaceWaterMap, surfaceWaterMap);
        }
    }

    private void soilWaterFlow()
    {

    }

    private void surfaceWaterVelocity()
    {
        surfaceComputeShader.SetFloat("pipeLength", cellHeight);
        surfaceComputeShader.SetFloat("epsilon", epsilon);

        surfaceComputeShader.SetTexture(kernel_velocity, "waterMap", surfaceWaterMap);
        surfaceComputeShader.SetTexture(kernel_velocity, "newWaterMap", newSurfaceWaterMap);
        surfaceComputeShader.SetTexture(kernel_velocity, "flowMap", surfaceFlowMap);
        surfaceComputeShader.SetTexture(kernel_velocity, "velocityMap", surfaceVelocityMap);

        DispatchCompute(kernel_velocity);

        if (enableVelocityDiffusion)
        {
            const float viscosity = 10.5f;
            const int iterations = 2;
            diffuseAlpha = cellArea / (viscosity * timeStep);

            surfaceComputeShader.SetFloat("diffuseAlpha", diffuseAlpha);

            surfaceComputeShader.SetTexture(kernel_diffusion, "velocityMap", surfaceVelocityMap);
            surfaceComputeShader.SetTexture(kernel_diffusion, "newVelocityMap", newSurfaceVelocityMap);

            for (int i = 0; i < iterations; i++)
            {
                DispatchCompute(kernel_diffusion);
                Graphics.Blit(newSurfaceVelocityMap, surfaceVelocityMap);
            }
        }
    }


    public void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        switch (textureToDraw)
        {
            case 1:
                Graphics.Blit(result, dest);
                break;
            case 2:
                Graphics.Blit(surfaceFlowMap, dest);
                break;
            case 3:
                Graphics.Blit(surfaceVelocityMap, dest);
                break;
            case 4:
                Graphics.Blit(soilSaturationMap, dest);
                break;
            case 5:
                Graphics.Blit(soilFlowMap, dest);
                break;
            case 6:
                Graphics.Blit(heightmap, dest);
                break;
            default:
                Graphics.Blit(src, dest);
                break;
        }
    }

    public void OnGUI()
    {
        if (showDebugTextures)
        {
            GUI.Label(new Rect(0, 256, 200, 50), "Water Height");
            GUI.DrawTexture(new Rect(0, 0, 256, 256), surfaceWaterMap, ScaleMode.ScaleToFit, false);
            GUI.Label(new Rect(256, 256, 100, 50), "Water Flow");
            GUI.DrawTexture(new Rect(256, 0, 256, 256), surfaceFlowMap, ScaleMode.ScaleToFit, false);
            GUI.Label(new Rect(512, 256, 200, 50), "Water Velocity");
            GUI.DrawTexture(new Rect(512, 0, 256, 256), surfaceVelocityMap, ScaleMode.ScaleToFit, false);
            GUI.Label(new Rect(768, 256, 200, 50), "Heightmap");
            GUI.DrawTexture(new Rect(768, 0, 256, 256), heightmap, ScaleMode.ScaleToFit, false);
        }

        string drawingTextureName = "Drawing: ";
        switch (textureToDraw)
        {
            case 0:
                drawingTextureName += "Water Height";
                break;
            case 1:
                drawingTextureName += "Water Flow";
                break;
            case 2:
                drawingTextureName += "Water Velocity";
                break;
            case 3:
                drawingTextureName += "Heightmap";
                break;
        }
        GUI.Label(new Rect(0, 300, 200, 50), drawingTextureName);

        if (GUI.Button(new Rect(0, 512, 100, 50), "Reset"))
        {
            DispatchCompute(kernel_reset);
        }
    }
}