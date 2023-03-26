using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    private int[] cells;

    public int resolution = 256;

    public int brushSize = 8;
    public Vector2 brushPosition = new Vector2(0, 0);
    public bool drawing = false;

    public int filledCount = 0;
    public int filledAvg = 0;

    private void Awake()
    {
        cells = new int[resolution * resolution];
        for (int i = 0; i < resolution * resolution; i++)
        {
            cells[i] = 0;
        }
    }

    public void ComputeWater()
    {
        int intSize = sizeof(int);
        int totalSize = intSize;

        ComputeBuffer cellsBuffer = new ComputeBuffer(cells.Length, totalSize);
        cellsBuffer.SetData(cells);

        computeShader.SetBuffer(0, "cells", cellsBuffer);
        computeShader.SetTexture(0, "output", renderTexture);
        computeShader.SetInt("resolution", resolution);
        computeShader.SetInt("brushSize", brushSize);
        computeShader.SetInt("brushX", (int) brushPosition.x);
        computeShader.SetInt("brushY", (int) brushPosition.y);
        computeShader.SetInt("drawing", drawing ? 1 : 0);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

        cellsBuffer.GetData(cells);

        cellsBuffer.Dispose();

        filledCount = 0;
        int sum = 0;
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] > 0)
            {
                filledCount++;
                sum += cells[i];
            }
        }

        filledAvg = sum / filledCount;
    }

    public void FixedUpdate()
    {
        // Add water at the mouse position when clicking
        if (Input.GetMouseButton(0))
        {
            drawing = true;
            brushPosition = Input.mousePosition;
            brushPosition.x = brushPosition.x / Screen.width * resolution;
            brushPosition.y = brushPosition.y / Screen.height * resolution;
        } else {
            drawing = false;
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(resolution, resolution, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        ComputeWater();

        Graphics.Blit(renderTexture, dest);
    }

    private void OnGUI()
    {
        if (cells != null)
        {
            if (GUI.Button(new Rect(0, 60, 100, 50), "Clear"))
            {
                for (int i = 0; i < resolution * resolution; i++)
                {
                    cells[i] = 0;
                }
            }

            if (GUI.Button(new Rect(0, 120, 100, 50), "Add water"))
            {
                // Fill a circle in the center of the screen
                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        int index = y * resolution + x;
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(resolution / 2, resolution / 2)) < 10)
                        {
                            cells[index] = 10;
                        }
                    }
                }
            }
        }
    }
}