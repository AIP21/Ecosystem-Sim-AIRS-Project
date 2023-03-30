using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    // Public
    [Header("References")]
    public ComputeShader computeShader;
    public RenderTexture RT;
    public RenderTexture newRT;
    public RenderTexture outputRT;
    public Texture2D heightmap;

    [Header("Settings")]
    public int resolution = 256;
    public float diffusionFactor = 0.1f;
    [Range(0, 25)]
    public int brushSize = 8;

    public int maxValue = 10;

    [Header("Water source")]
    public float sourceRate = 1;
    [Range(0, 25)]
    public int sourceSize = 4;
    public int sourceAmount = 1;
    public Vector2Int sourcePosition;

    // Private
    private float lastSourceTime = 0;

    private void Awake()
    {
        sourcePosition = new Vector2Int(resolution / 2, resolution / 5);

        if (outputRT == null)
        {
            outputRT = new RenderTexture(resolution, resolution, 24);
            outputRT.enableRandomWrite = true;
            outputRT.Create();
        }
        if (RT == null)
        {
            RT = new RenderTexture(resolution, resolution, 24);
            RT.enableRandomWrite = true;
            RT.Create();
        }
        if (newRT == null)
        {
            newRT = new RenderTexture(resolution, resolution, 24);
            newRT.enableRandomWrite = true;
            newRT.Create();
        }
    }

    public void FixedUpdate()
    {
        if (outputRT == null)
        {
            outputRT = new RenderTexture(resolution, resolution, 24);
            outputRT.enableRandomWrite = true;
            outputRT.Create();
        }
        if (RT == null)
        {
            RT = new RenderTexture(resolution, resolution, 24);
            RT.enableRandomWrite = true;
            RT.Create();
        }
        if (newRT == null)
        {
            newRT = new RenderTexture(resolution, resolution, 24);
            newRT.enableRandomWrite = true;
            newRT.Create();
        }

        computeShader.SetInt("fillX", 0);
        computeShader.SetInt("fillY", 0);
        computeShader.SetInt("fillRadius", 0);
        computeShader.SetInt("resolution", resolution);
        computeShader.SetInt("maxWater", maxValue);
        computeShader.SetInt("filling", 0);
        computeShader.SetFloat("diffusionFactor", diffusionFactor);
        computeShader.SetTexture(0, "Output", outputRT);
        computeShader.SetTexture(0, "Watermap", RT);
        computeShader.SetTexture(0, "NewWatermap", newRT);
        computeShader.SetTexture(0, "Heightmap", heightmap);

        // Copy newRT to RT
        Graphics.Blit(newRT, RT);

        // Add water at the mouse position when clicking
        if (Input.GetMouseButton(0))
        {
            fillCircle((int)(Input.mousePosition.x / Screen.width * resolution), (int)(Input.mousePosition.y / Screen.height * resolution), brushSize);
        }

        // Add water from source
        if (sourceRate != 0 && lastSourceTime + sourceRate < Time.time)
        {
            fillCircle(sourcePosition.x, sourcePosition.y, sourceSize, sourceAmount);
            lastSourceTime = Time.time;
        }

        computeShader.Dispatch(0, RT.width / 8, RT.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(outputRT == null)
        {
            outputRT = new RenderTexture(resolution, resolution, 24);
            outputRT.enableRandomWrite = true;
            outputRT.Create();
        }
        if (RT == null)
        {
            RT = new RenderTexture(resolution, resolution, 24);
            RT.enableRandomWrite = true;
            RT.Create();
        }
        if (newRT == null)
        {
            newRT = new RenderTexture(resolution, resolution, 24);
            newRT.enableRandomWrite = true;
            newRT.Create();
        }

        Graphics.Blit(outputRT, dest);
    }

    private void fillCircle(int xCenter, int yCenter, int size, int amount = 10)
    {
        computeShader.SetInt("fillX", xCenter);
        computeShader.SetInt("fillY", yCenter);
        computeShader.SetInt("fillRadius", size);
        computeShader.SetInt("filling", 1);
    }

    public void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Clear"))
        {
            outputRT = new RenderTexture(resolution, resolution, 24);
            outputRT.enableRandomWrite = true;
            outputRT.Create();
            RT = new RenderTexture(resolution, resolution, 24);
            RT.enableRandomWrite = true;
            RT.Create();
            newRT = new RenderTexture(resolution, resolution, 24);
            newRT.enableRandomWrite = true;
            newRT.Create();
        }
    }
}