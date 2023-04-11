using UnityEngine;
using SimDataStructure;
using System;


namespace WaterSim
{
    [Serializable]
    public class WaterSimGridData : AbstractGridData
    {
        public override void InitComputeShader()
        {
            throw new System.NotImplementedException();
        }

        public override void CreateComputeBuffers()
        {
            throw new System.NotImplementedException();
        }

        public override void CreateRenderTextures()
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateShader(float deltaTime)
        {
            throw new System.NotImplementedException();
        }
    }
}