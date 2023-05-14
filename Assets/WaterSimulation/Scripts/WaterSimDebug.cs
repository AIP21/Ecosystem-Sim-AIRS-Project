using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimDataStructure.Interfaces;
using Managers.Interfaces;
using SimDataStructure.Data;

namespace WaterSim
{
    public class WaterSimDebug : MonoBehaviour, ITickableSystem, IReadGridData
    {
        [Space(10)]
        [Range(0, 7)]
        public int textureToDraw = 0;

        public bool showDebugTextures = true;

        [Tooltip("Should be the same as the resolution of the water simulation")]
        public int resolution = 1024;

        [Space(10)]
        public RenderTexture waterMap;
        public RenderTexture flowMap;
        public RenderTexture velocityMap;

        // Soil Water
        [Space(10)]
        public RenderTexture soilUseMap; // where water is being consumed (by trees, plants, etc.)
        public RenderTexture saturationMap;

        #region Interface Stuff
        public float TickPriority { get { return 1.1f; } }
        public int TickInterval { get { return 5; } }
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
        public bool shouldTick { get { return this.isActiveAndEnabled; } }

        private Dictionary<string, int> _readDataNames = new Dictionary<string, int>() {
            { "waterHeight", 0 },
            { "waterFlow", 0 },
            { "waterVelocity", 0 },
            { "soilSaturation", 1 }, // 2
            { "soilUse", 2 } // 4
        };  // The names of the data this is reading from the data structure
        public Dictionary<string, int> ReadDataNames { get { return _readDataNames; } }
        #endregion

        private void Awake()
        {
            // Create render textures
            waterMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
            flowMap = createTexture(RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);
            velocityMap = createTexture(RenderTextureFormat.RGFloat, FilterMode.Bilinear);
            saturationMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
            soilUseMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
        }

        public void receiveData(List<AbstractGridData> sentData)
        {
            int i = 0;

            foreach (string name in ReadDataNames.Keys)
            {
                AbstractGridData abstractData = sentData[i];
                TextureGridData dat = abstractData is TextureGridData ? (TextureGridData)abstractData : null;

                if (dat == null)
                {
                    print("Received NULL data");
                    continue;
                }

                // Copy the received data to the appropriate texture
                switch (name)
                {
                    case "waterHeight":
                        // print("Receiving waterHeight map");
                        dat.GetData(waterMap);

                        break;
                    case "waterFlow":
                        // print("Receiving waterFlow map");
                        dat.GetData(flowMap);

                        break;
                    case "waterVelocity":
                        // print("Receiving waterVelocity map");
                        dat.GetData(velocityMap);

                        break;
                    case "soilSaturation":
                        // print("Receiving soilSaturation map");
                        dat.GetData(saturationMap);

                        break;
                    case "soilUse":
                        // print("Receiving soilUse map");
                        dat.GetData(soilUseMap);

                        break;
                }

                i++;
            }
        }

        public void BeginTick(float deltaTime)
        {

        }

        public void Tick(float deltaTime)
        {

        }

        public void EndTick(float deltaTime)
        {

        }

        public void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            switch (textureToDraw)
            {
                case 1:
                    Graphics.Blit(flowMap, dest);
                    break;
                case 2:
                    Graphics.Blit(waterMap, dest);
                    break;
                case 3:
                    Graphics.Blit(saturationMap, dest);
                    break;
                case 4:
                    Graphics.Blit(velocityMap, dest);
                    break;
                case 5:
                    Graphics.Blit(soilUseMap, dest);
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
                int debugTextureSize = 256;

                GUI.Label(new Rect(0, debugTextureSize, 200, 50), "Water Map");
                GUI.DrawTexture(new Rect(0, 0, debugTextureSize, debugTextureSize), waterMap, ScaleMode.ScaleToFit, false);

                GUI.Label(new Rect(debugTextureSize, debugTextureSize, 100, 50), "Water Flow");
                GUI.DrawTexture(new Rect(debugTextureSize, 0, debugTextureSize, debugTextureSize), flowMap, ScaleMode.ScaleToFit, false);

                GUI.Label(new Rect(debugTextureSize * 2, debugTextureSize, 200, 50), "Water Velocity");
                GUI.DrawTexture(new Rect(debugTextureSize * 2, 0, debugTextureSize, debugTextureSize), velocityMap, ScaleMode.ScaleToFit, false);

                GUI.Label(new Rect(debugTextureSize * 3, debugTextureSize, 200, 50), "Soil Saturation");
                GUI.DrawTexture(new Rect(debugTextureSize * 3, 0, debugTextureSize, debugTextureSize), saturationMap, ScaleMode.ScaleToFit, false);
            }

            string drawingTextureName = "Drawing: ";
            switch (textureToDraw)
            {
                case 1:
                    drawingTextureName += "Surface Water Flow Map";
                    break;
                case 2:
                    drawingTextureName += "Water Data Map";
                    break;
                case 3:
                    drawingTextureName += "Soil Water Saturation Map";
                    break;
                case 4:
                    drawingTextureName += "Surface Water Velocity Map";
                    break;
                case 5:
                    drawingTextureName += "Soil Use Map";
                    break;
                default:
                    drawingTextureName += "???";
                    break;
            }
            GUI.Label(new Rect(0, 300, 200, 50), drawingTextureName);

        }

        private RenderTexture createTexture(RenderTextureFormat format, FilterMode filterMode = FilterMode.Point)
        {
            RenderTexture rt = new RenderTexture(resolution, resolution, 24, format);
            rt.filterMode = filterMode;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.enableRandomWrite = true;
            rt.Create();

            return rt;
        }
    }
}