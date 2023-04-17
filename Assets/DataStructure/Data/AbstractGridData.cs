using UnityEngine;
using System;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    // Data for whole grid (one instance per grid)
    // This class will contain a reference to a compute shader that will be a grid shader that computes some data for each cell
    // This class will contain a method to update the shader
    // This class will contain a method to access the data of the shader
    // This class will contain a method to supply input data to the shader
    [Serializable]
    public abstract class AbstractGridData
    {
        public ComputeShader computeShader;

        public GenericDictionary<string, ComputeBuffer> computeBuffers = new GenericDictionary<string, ComputeBuffer>();
        public GenericDictionary<string, RenderTexture> renderTextures = new GenericDictionary<string, RenderTexture>();

        // This class will contain a reference to a compute shader that will be a grid shader that computes some data for each cell
        // This class will contain a method to update the shader
        // This class will contain a method to access the data of the shader
        // This class will contain a method to supply input data to the shader
        public AbstractGridData()
        {
        }

        public virtual void Init()
        {
            this.InitComputeShader();
            this.CreateComputeBuffers();
            this.CreateRenderTextures();
        }

        public virtual void Tick(float deltaTime)
        {
            this.UpdateShader(deltaTime);
        }

        public abstract void InitComputeShader();

        public abstract void CreateRenderTextures();

        public abstract void CreateComputeBuffers();

        public abstract void UpdateShader(float deltaTime);
    }
}