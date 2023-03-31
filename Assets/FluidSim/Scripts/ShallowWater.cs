using System;
using System.Collections;
using UnityEngine;

public class ShallowWater : MonoBehaviour
{
    #region Public
    [Header("References")]
    public ComputeShader computeShader;
    public Texture2D heightmap;

    #region RenderTextures
    [Header("RenderTextures")]
    public RenderTexture waterMap;
    public RenderTexture newWaterMap;
    public RenderTexture flowMap;
    public RenderTexture newFlowMap;
    public RenderTexture velocityMap;
    public RenderTexture newVelocityMap;

    [Space(10)]
    [Range(0, 3)]
    public int TextureToDraw = 0;
    #endregion

    #region Settings
    [Header("Settings")]
    public int resolution = 1024;
    public float timeStep = 0.1f;
    public float waterDensity = 1.0f;
    public float gravitationAcceleration = 9.81f;
    public float cellHeight = 1;
    public float cellArea = 1; // should be the cell height squared
    public float epsilon = 1e-5f;
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

    [Header("Water source")]
    public bool sourceIsMouse = false;
    [Range(0, 1)]
    public float sourceRadius = 0.008f;
    public float sourceAmount = 1;
    public Vector2 sourcePosition;
    #endregion

    [Space(25)]
    public bool showDebugTextures = true;
    #endregion

    #region Private
    private int dispatchSize = 0;
    private int kernelCount = 0;
    private int kernel_reset = 0;
    private int kernel_input = 0;
    private int kernel_evaporation = 0;
    private int kernel_freeSlip = 0;
    private int kernel_flow = 0;
    private int kernel_height = 0;
    private int kernel_velocity = 0;
    private int kernel_diffusion = 0;
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

    private void DispatchCompute(int kernel)
    {
        computeShader.Dispatch(kernel, dispatchSize, dispatchSize, 1);
    }

    private void Awake()
    {
        sourcePosition = new Vector2(0.5f, 0.5f);

        // Create render textures
        waterMap = CreateTexture(RenderTextureFormat.RFloat);
        newWaterMap = CreateTexture(RenderTextureFormat.RFloat);
        flowMap = CreateTexture(RenderTextureFormat.ARGBHalf);
        newFlowMap = CreateTexture(RenderTextureFormat.ARGBHalf);
        velocityMap = CreateTexture(RenderTextureFormat.RGHalf, FilterMode.Bilinear);
        newVelocityMap = CreateTexture(RenderTextureFormat.RGHalf, FilterMode.Bilinear);

        // Set shader variables
        computeShader.SetFloat("waterDensity", waterDensity);
        computeShader.SetFloat("gravityAcceleration", gravitationAcceleration);
        computeShader.SetFloat("pipeLength", cellHeight);
        computeShader.SetFloat("pipeArea", cellArea);
        computeShader.SetFloat("epsilon", epsilon);
        computeShader.SetFloat("diffuseAlpha", diffuseAlpha);
        computeShader.SetVector("inputPosition", sourcePosition);
        computeShader.SetFloat("inputRadius", sourceRadius);
        computeShader.SetFloat("inputAmount", sourceAmount);
        computeShader.SetFloat("flowDamping", 1.0f - flowDamping);
        computeShader.SetFloat("_deltaTime", timeStep);
        computeShader.SetFloat("heightmapMultiplier", heightmapMultiplier);
        computeShader.SetFloat("evaporationConstant", evaporationConstant);
        computeShader.SetFloat("size", resolution);

        // Find shader kernels
        kernel_reset = computeShader.FindKernel("reset"); kernelCount++;
        kernel_input = computeShader.FindKernel("computeInput"); kernelCount++;
        kernel_evaporation = computeShader.FindKernel("computeEvaporation"); kernelCount++;
        kernel_flow = computeShader.FindKernel("computeWaterFlow"); kernelCount++;
        kernel_height = computeShader.FindKernel("computeWaterHeight"); kernelCount++;
        kernel_freeSlip = computeShader.FindKernel("applyFreeSlip"); kernelCount++;
        kernel_velocity = computeShader.FindKernel("computeWaterVelocity"); kernelCount++;
        kernel_diffusion = computeShader.FindKernel("computeDiffusedWaterVelocity"); kernelCount++;

        // Setup shader render textures
        updateTextures();

        // Initialize shader textures
        dispatchSize = Mathf.CeilToInt(resolution / 8);
        DispatchCompute(kernel_reset);
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
            computeShader.SetTexture(kernel, "heightmap", heightmap);
            computeShader.SetTexture(kernel, "waterMap", waterMap);
            computeShader.SetTexture(kernel, "newWaterMap", newWaterMap);
            computeShader.SetTexture(kernel, "flowMap", flowMap);
            computeShader.SetTexture(kernel, "newFlowMap", newFlowMap);
            computeShader.SetTexture(kernel, "velocityMap", velocityMap);
            computeShader.SetTexture(kernel, "newVelocityMap", newVelocityMap);
        }
    }

    public void Update()
    {
        // Set shader variables
        computeShader.SetFloat("waterDensity", waterDensity);
        computeShader.SetFloat("epsilon", epsilon);
        computeShader.SetFloat("diffuseAlpha", diffuseAlpha);
        computeShader.SetFloat("_deltaTime", timeStep);
        computeShader.SetFloat("heightmapMultiplier", heightmapMultiplier);

        if (enableInput)
            handleInput();

        applyFreeSlip();

        waterFlow();

        if (enableVelocity)
            waterVelocity();
    }

    private void handleInput()
    {
        if (sourceIsMouse)
        {
            // Add water at the mouse position while clicking
            computeShader.SetVector("inputPosition", new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.width));
            computeShader.SetFloat("inputRadius", sourceRadius);
            computeShader.SetFloat("inputAmount", Input.GetMouseButton(0) ? sourceAmount : 0);
        }
        else
        {
            // Add water at the source position
            computeShader.SetVector("inputPosition", sourcePosition);
            computeShader.SetFloat("inputRadius", sourceRadius);
            computeShader.SetFloat("inputAmount", sourceAmount);
        }

        computeShader.SetTexture(kernel_input, "waterMap", waterMap);
        computeShader.SetTexture(kernel_input, "newWaterMap", newWaterMap);

        DispatchCompute(kernel_input);

        Graphics.Blit(newWaterMap, waterMap);

        if (evaporationConstant > 0.0f)
        {
            computeShader.SetFloat("evaporationConstant", evaporationConstant);

            computeShader.SetTexture(kernel_evaporation, "waterMap", waterMap);
            computeShader.SetTexture(kernel_evaporation, "newWaterMap", newWaterMap);

            DispatchCompute(kernel_evaporation);

            Graphics.Blit(newWaterMap, waterMap);
        }
    }

    private void applyFreeSlip()
    {
        // This function prevents the water from freaking out when it hits a border
        computeShader.SetTexture(kernel_freeSlip, "waterMap", waterMap);
        computeShader.SetTexture(kernel_freeSlip, "newWaterMap", newWaterMap);

        DispatchCompute(kernel_freeSlip);
        Graphics.Blit(newWaterMap, waterMap);
    }

    private void waterFlow()
    {
        if (enableFlow)
        {
            computeShader.SetFloat("gravityAcceleration", gravitationAcceleration);
            computeShader.SetFloat("pipeLength", cellHeight);
            computeShader.SetFloat("pipeArea", cellArea);
            computeShader.SetFloat("flowDamping", 1.0f - flowDamping);

            computeShader.SetTexture(kernel_flow, "heightmap", heightmap);
            computeShader.SetTexture(kernel_flow, "waterMap", waterMap);
            computeShader.SetTexture(kernel_flow, "flowMap", flowMap);
            computeShader.SetTexture(kernel_flow, "newFlowMap", newFlowMap);

            DispatchCompute(kernel_flow);

            Graphics.Blit(newFlowMap, flowMap);
        }

        if (enableHeight)
        {
            computeShader.SetFloat("pipeLength", cellHeight);

            computeShader.SetTexture(kernel_height, "waterMap", waterMap);
            computeShader.SetTexture(kernel_height, "newWaterMap", newWaterMap);
            computeShader.SetTexture(kernel_height, "flowMap", flowMap);

            DispatchCompute(kernel_height);

            Graphics.Blit(newWaterMap, waterMap);
        }
    }

    private void waterVelocity()
    {
        computeShader.SetFloat("pipeLength", cellHeight);
        computeShader.SetFloat("epsilon", epsilon);

        computeShader.SetTexture(kernel_velocity, "waterMap", waterMap);
        computeShader.SetTexture(kernel_velocity, "newWaterMap", newWaterMap);
        computeShader.SetTexture(kernel_velocity, "flowMap", flowMap);
        computeShader.SetTexture(kernel_velocity, "velocityMap", velocityMap);

        DispatchCompute(kernel_velocity);

        if (enableVelocityDiffusion)
        {
            const float viscosity = 10.5f;
            const int iterations = 2;
            diffuseAlpha = cellArea / (viscosity * timeStep);

            computeShader.SetFloat("diffuseAlpha", diffuseAlpha);

            computeShader.SetTexture(kernel_diffusion, "velocityMap", velocityMap);
            computeShader.SetTexture(kernel_diffusion, "newVelocityMap", newVelocityMap);

            for (int i = 0; i < iterations; i++)
            {
                DispatchCompute(kernel_diffusion);
                Graphics.Blit(newVelocityMap, velocityMap);
            }
        }
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        switch (TextureToDraw)
        {
            case 0:
                Graphics.Blit(waterMap, dest);
                break;
            case 1:
                Graphics.Blit(flowMap, dest);
                break;
            case 2:
                Graphics.Blit(velocityMap, dest);
                break;
            case 3:
                Graphics.Blit(heightmap, dest);
                break;
        }
    }

    public void OnGUI()
    {
        if (showDebugTextures)
        {
            GUI.Label(new Rect(0, 256, 200, 50), "Water Height");
            GUI.DrawTexture(new Rect(0, 0, 256, 256), waterMap, ScaleMode.ScaleToFit, false);
            GUI.Label(new Rect(256, 256, 100, 50), "Water Flow");
            GUI.DrawTexture(new Rect(256, 0, 256, 256), flowMap, ScaleMode.ScaleToFit, false);
            GUI.Label(new Rect(512, 256, 200, 50), "Water Velocity");
            GUI.DrawTexture(new Rect(512, 0, 256, 256), velocityMap, ScaleMode.ScaleToFit, false);
            GUI.Label(new Rect(768, 256, 200, 50), "Heightmap");
            GUI.DrawTexture(new Rect(768, 0, 256, 256), heightmap, ScaleMode.ScaleToFit, false);
        }

        string drawingTextureName = "Drawing: ";
        switch (TextureToDraw)
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