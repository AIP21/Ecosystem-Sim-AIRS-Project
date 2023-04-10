using UnityEngine;
using SimDataStructure;

namespace WaterSim
{
    [Serializable]
    public class WaterSimGridData : AbstractGridData
    {
        public override void Init()
        {
            base.Init();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

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

        public override void UpdateShader()
        {
            throw new System.NotImplementedException();
        }
    }
}