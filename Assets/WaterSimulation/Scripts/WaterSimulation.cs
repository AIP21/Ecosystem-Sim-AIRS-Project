using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;

namespace WaterSim
{
    public class WaterSimulation : MonoBehaviour, ITickableSystem, IReadGridData, IWriteGridData, ISetupGridData
    {
        #region Public
        [Header("Compute Shader")]
        public ComputeShader computeShader;

        #region Render Textures
        [Header("Render Textures")]
        // Surface Water
        public RenderTexture result;

        public RenderTexture waterMap;
        public RenderTexture DSWaterMap;
        public RenderTexture newWaterMap;
        public RenderTexture flowMap;
        public RenderTexture DSFlowMap;
        public RenderTexture newFlowMap;
        public RenderTexture velocityMap;
        public RenderTexture newVelocityMap;
        public RenderTexture DSVelocityMap;

        // Soil Water
        [Space(10)]
        public RenderTexture soilUseMap; // where water is being consumed (by trees, plants, etc.)
        public RenderTexture newSoilUseMap; // where water is being consumed (by trees, plants, etc.)
        public RenderTexture saturationMap;
        public RenderTexture newSaturationMap;
        public RenderTexture DSSaturationMap;

        #endregion

        #region General Settings
        [Header("General Settings")]
        public Texture2D heightmap;

        [Space(10)]
        public int resolution = 1024;
        public int externalResolution = 256;
        public float timeStep = 0.1f; // if multiplier: 7.4?
        public float epsilon = 1e-5f;
        #endregion

        #region Surface Water Settings
        [Header("Surface Water Settings")]
        public float waterDensity = 1.0f;
        public float gravitationAcceleration = 9.81f;
        public float cellHeight = 1;
        public float cellArea = 1; // should be the cell height squared
        public float diffuseAlpha = 1.0f;
        public float evaporationConstant = 0.000001f;
        [Range(-20, 20)]
        public float heightmapMultiplier = 1.0f;
        public float flowDamping = 0.0f;
        public float viscosity = 10.5f;
        public int diffusionIterations = 2;

        [Space(10)]
        public bool enableWaterFlux = true;
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
        // public ComputeShader computeShader;
        public Texture2D soilDataMap; // water capacity (affects absorption rate (absorptivity), evaporation rate, flow rate, and max holding capacity)

        [Space(10)]

        public float soilEvaporationConstant = 0.0005f;
        public float soilDiffusionConstant = 0.1f;

        public float soilAbsorptionMultiplier = 0.0005f;
        public float soilUseMultiplier = 1.0f;
        public float soilReleaseMultiplier = 1.0f;

        [Tooltip("The minimum amount of soil water that there can be for the soil to be able to release water.")]
        public float soilReleaseThreshold = 0.5f;
        [Tooltip("The maximum amount of surface water that can be above soil for the soil to still be able to release water.")]
        public float soilReleaseSurfaceThreshold = 0.1f;

        [Space(10)]
        public bool enableSoilUse = true;
        public bool enableSoilAbsorption = true;
        public bool enableSoilRelease = true;
        public bool enableSoilEvaporation = true;
        public bool enableSoilDiffusion = true;
        #endregion

        // [Header("Data Structure")]
        private Dictionary<string, int> _readDataNames = new Dictionary<string, int>() {
            { "waterHeight", 0 },
            { "waterFlow", 0 },
            { "waterVelocity", 0 },
            { "soilSaturation", 1 }, // 2
            { "soilUse", 2 } // 4
        };  // The names of the data this is reading from the data structure
        public Dictionary<string, int> ReadDataNames { get { return _readDataNames; } }

        private Dictionary<string, int> _writeDataNames = new Dictionary<string, int>(){
            { "waterHeight", 0 },
            { "waterFlow", 0 },
            { "waterVelocity", 0 },
            { "soilSaturation", 1 }, // 2
            { "soilUse", 2 } // 4
        };  // The names of the data this is writing to the data structure
        public Dictionary<string, int> WriteDataNames { get { return _writeDataNames; } }
        #endregion

        #region Private
        private int dispatchSize = 0;
        private int kernelCount = 0;
        private int kernel_reset = 0;
        private int kernel_flux = 0;
        private int kernel_slipPass = 0;
        private int kernel_flow = 0;
        private int kernel_height = 0;
        private int kernel_velocity = 0;
        private int kernel_diffusion = 0;

        #region Interface Stuff
        public float TickPriority { get { return 1; } }
        public int TickInterval { get { return 5; } }
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
        public bool shouldTick { get { return this.isActiveAndEnabled; } }
        #endregion
        #endregion

        private void Awake()
        {
            sourcePosition = new Vector2(0.5f, 0.5f);

            // Create render textures
            result = createTexture(RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);

            // Create data
            waterMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
            newWaterMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
            DSWaterMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);

            flowMap = createTexture(RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);
            newFlowMap = createTexture(RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);
            DSFlowMap = createTexture(RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);

            velocityMap = createTexture(RenderTextureFormat.RGFloat, FilterMode.Bilinear);
            newVelocityMap = createTexture(RenderTextureFormat.RGFloat, FilterMode.Bilinear);
            DSVelocityMap = createTexture(RenderTextureFormat.RGFloat, FilterMode.Bilinear);

            saturationMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
            newSaturationMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
            DSSaturationMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);

            soilUseMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);
            newSoilUseMap = createTexture(RenderTextureFormat.RFloat, FilterMode.Bilinear);

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
            computeShader.SetFloat("heightmapMultiplier", heightmapMultiplier);
            computeShader.SetFloat("evaporationConstant", evaporationConstant);
            computeShader.SetFloat("size", resolution);
            computeShader.SetFloat("externalSize", externalResolution);
            computeShader.SetFloat("soilEvaporationConstant", soilEvaporationConstant);
            computeShader.SetFloat("soilAbsorptionMultiplier", soilAbsorptionMultiplier);
            computeShader.SetFloat("soilUseMultiplier", soilUseMultiplier);
            computeShader.SetFloat("soilReleaseMultiplier", soilReleaseMultiplier);
            computeShader.SetFloat("soilReleaseThreshold", soilReleaseThreshold);
            computeShader.SetFloat("soilReleaseSurfaceThreshold", soilReleaseSurfaceThreshold);
            computeShader.SetFloat("soilDiffusionConstant", soilDiffusionConstant);

            // Find shader kernels
            kernel_reset = computeShader.FindKernel("reset"); kernelCount++;
            kernel_flux = computeShader.FindKernel("computeFlux"); kernelCount++;
            kernel_flow = computeShader.FindKernel("computeFlow"); kernelCount++;
            kernel_height = computeShader.FindKernel("computeWaterHeight"); kernelCount++;
            kernel_slipPass = computeShader.FindKernel("applySlipPass"); kernelCount++;
            kernel_velocity = computeShader.FindKernel("computeWaterVelocity"); kernelCount++;
            kernel_diffusion = computeShader.FindKernel("computeDiffusedWaterVelocity"); kernelCount++;

            // Setup shader render textures for resetting them
            computeShader.SetTexture(kernel_reset, "worldDataMap", heightmap);
            computeShader.SetTexture(kernel_reset, "result", result);
            computeShader.SetTexture(kernel_reset, "waterMap", waterMap);
            computeShader.SetTexture(kernel_reset, "newWaterMap", newWaterMap);
            computeShader.SetTexture(kernel_reset, "saturationMap", saturationMap);
            computeShader.SetTexture(kernel_reset, "newSaturationMap", newSaturationMap);
            computeShader.SetTexture(kernel_reset, "flowMap", flowMap);
            computeShader.SetTexture(kernel_reset, "newFlowMap", newFlowMap);
            computeShader.SetTexture(kernel_reset, "velocityMap", velocityMap);
            computeShader.SetTexture(kernel_reset, "newVelocityMap", newVelocityMap);

            // Initialize shader textures
            dispatchSize = Mathf.CeilToInt(resolution / 8);

            DispatchCompute(kernel_reset);
        }

        #region Ticking
        public void BeginTick(float deltaTime)
        {
            // print("sim BeginTick");

            // Set shader variables
            computeShader.SetFloat("waterDensity", waterDensity);
            computeShader.SetFloat("epsilon", epsilon);
            computeShader.SetFloat("diffuseAlpha", diffuseAlpha);
            computeShader.SetFloat("_deltaTime", timeStep); // deltaTime
            computeShader.SetFloat("heightmapMultiplier", heightmapMultiplier);
        }

        public void Tick(float deltaTime)
        {
            // print("sim Tick");

            if (enableWaterFlux)
                waterFlux();

            applyFreeSlip();

            waterFlow();

            if (enableVelocity)
                surfaceWaterVelocity(timeStep);
        }

        public void EndTick(float deltaTime)
        {
            // print("\n--END OF TICK--\n");
        }
        #endregion

        #region Data Structure
        public Dictionary<Tuple<string, int>, AbstractGridData> initializeData()
        {
            Dictionary<Tuple<string, int>, AbstractGridData> data = new Dictionary<Tuple<string, int>, AbstractGridData>();

            foreach (string name in ReadDataNames.Keys)
            {
                switch (name)
                {
                    case "waterHeight":
                        data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RFloat, FilterMode.Bilinear));
                        break;
                    case "waterFlow":
                        data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear));
                        break;
                    case "waterVelocity":
                        data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RGFloat, FilterMode.Bilinear));
                        break;
                    case "soilSaturation":
                        data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RFloat, FilterMode.Bilinear));
                        break;
                    case "soilUse":
                        data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RFloat, FilterMode.Bilinear));
                        break;
                    default:
                        break;
                }
            }

            return data;
        }

        public void receiveData(List<AbstractGridData> data)
        {
            int i = 0;

            foreach (string name in ReadDataNames.Keys)
            {
                AbstractGridData abstractData = data[i];
                TextureGridData dat = abstractData is TextureGridData ? (TextureGridData)abstractData : null;

                if (dat == null)
                {
                    print("Received NULL data");
                    continue;
                }
                // else if (dat.GetData() == null)
                // {
                //     print("Received data has NULL data");
                //     continue;
                // }

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

        public Dictionary<Tuple<string, int>, object> writeData()
        {
            Dictionary<Tuple<string, int>, object> toWrite = new Dictionary<Tuple<string, int>, object>();

            foreach (string name in WriteDataNames.Keys)
            {
                switch (name)
                {
                    case "waterHeight":
                        // print("Writing waterHeight map");
                        toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newWaterMap);

                        break;
                    case "waterFlow":
                        // print("Writing waterFlow map");
                        toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newFlowMap);

                        break;
                    case "waterVelocity":
                        // print("Writing waterVelocity map");
                        toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newVelocityMap);

                        break;
                    case "soilSaturation":
                        // print("Writing soilSaturation map");
                        toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newSaturationMap);

                        break;
                    case "soilUse":
                        // print("Writing soilUse map");
                        toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newSoilUseMap);

                        break;
                }
            }

            return toWrite;
        }
        #endregion

        #region Simulation Processes
        private void waterFlux()
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

            computeShader.SetBool("soilAbsorption", enableSoilAbsorption);
            computeShader.SetBool("soilUse", enableSoilUse);
            computeShader.SetBool("soilEvaporation", enableSoilEvaporation);
            computeShader.SetBool("soilRelease", enableSoilRelease);
            computeShader.SetFloat("evaporationConstant", evaporationConstant);

            computeShader.SetFloat("soilEvaporationConstant", soilEvaporationConstant);
            computeShader.SetFloat("soilAbsorptionMultiplier", soilAbsorptionMultiplier);
            computeShader.SetFloat("soilUseMultiplier", soilUseMultiplier);
            computeShader.SetFloat("soilReleaseMultiplier", soilReleaseMultiplier);
            computeShader.SetFloat("soilReleaseThreshold", soilReleaseThreshold);
            computeShader.SetFloat("soilReleaseSurfaceThreshold", soilReleaseSurfaceThreshold);

            computeShader.SetTexture(kernel_flux, "worldDataMap", heightmap);
            computeShader.SetTexture(kernel_flux, "waterMap", waterMap);
            computeShader.SetTexture(kernel_flux, "newWaterMap", newWaterMap);
            computeShader.SetTexture(kernel_flux, "saturationMap", saturationMap);
            computeShader.SetTexture(kernel_flux, "newSaturationMap", newSaturationMap);
            computeShader.SetTexture(kernel_flux, "soilDataMap", soilDataMap);
            computeShader.SetTexture(kernel_flux, "soilUseMap", soilUseMap);
            computeShader.SetTexture(kernel_flux, "newSoilUseMap", newSoilUseMap);

            DispatchCompute(kernel_flux);

            Graphics.Blit(newWaterMap, waterMap);
            Graphics.Blit(newSoilUseMap, soilUseMap);
            Graphics.Blit(newSaturationMap, saturationMap);
        }

        private void applyFreeSlip()
        {
            // This function prevents the water from freaking out when it hits a border
            computeShader.SetTexture(kernel_slipPass, "waterMap", waterMap);
            computeShader.SetTexture(kernel_slipPass, "newWaterMap", newWaterMap);
            computeShader.SetTexture(kernel_slipPass, "saturationMap", saturationMap);
            computeShader.SetTexture(kernel_slipPass, "newSaturationMap", newSaturationMap);

            DispatchCompute(kernel_slipPass);

            Graphics.Blit(newWaterMap, waterMap);
            Graphics.Blit(newSaturationMap, saturationMap);
        }

        private void waterFlow()
        {
            if (enableFlow || enableSoilDiffusion)
            {
                computeShader.SetBool("surfaceFlow", enableFlow);
                computeShader.SetBool("soilFlow", enableSoilDiffusion);

                computeShader.SetTexture(kernel_flow, "worldDataMap", heightmap);
                computeShader.SetTexture(kernel_flow, "waterMap", waterMap);

                if (enableFlow)
                {
                    computeShader.SetTexture(kernel_flow, "flowMap", flowMap);
                    computeShader.SetTexture(kernel_flow, "newFlowMap", newFlowMap);

                    computeShader.SetFloat("gravityAcceleration", gravitationAcceleration);
                    computeShader.SetFloat("pipeLength", cellHeight);
                    computeShader.SetFloat("pipeArea", cellArea);
                    computeShader.SetFloat("flowDamping", 1.0f - flowDamping);
                }

                if (enableSoilDiffusion)
                {
                    computeShader.SetFloat("soilDiffusionConstant", soilDiffusionConstant);

                    computeShader.SetTexture(kernel_flow, "saturationMap", saturationMap);
                    computeShader.SetTexture(kernel_flow, "newSaturationMap", newSaturationMap);
                }

                DispatchCompute(kernel_flow);

                if (enableFlow)
                    Graphics.Blit(newFlowMap, flowMap);

                if (enableSoilDiffusion)
                    Graphics.Blit(newSaturationMap, saturationMap);
            }

            // Determine the new surface water height from the flow
            if (enableHeight)
            {
                computeShader.SetFloat("pipeLength", cellHeight);

                computeShader.SetTexture(kernel_height, "worldDataMap", heightmap);
                computeShader.SetTexture(kernel_height, "flowMap", flowMap);
                computeShader.SetTexture(kernel_height, "waterMap", waterMap);
                computeShader.SetTexture(kernel_height, "newWaterMap", newWaterMap);
                computeShader.SetTexture(kernel_height, "result", result);
                computeShader.SetTexture(kernel_height, "saturationMap", saturationMap);

                DispatchCompute(kernel_height);

                Graphics.Blit(newWaterMap, waterMap);
            }
        }

        private void surfaceWaterVelocity(float deltaTime)
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
                diffuseAlpha = cellArea / (viscosity * deltaTime);

                computeShader.SetFloat("diffuseAlpha", diffuseAlpha);

                computeShader.SetTexture(kernel_diffusion, "velocityMap", velocityMap);
                computeShader.SetTexture(kernel_diffusion, "newVelocityMap", newVelocityMap);

                for (int i = 0; i < diffusionIterations; i++)
                {
                    DispatchCompute(kernel_diffusion);

                    Graphics.Blit(newVelocityMap, velocityMap);
                }
            }
        }
        #endregion

        #region Utilities
        private RenderTexture createTexture(RenderTextureFormat format, FilterMode filterMode = FilterMode.Point)
        {
            RenderTexture rt = new RenderTexture(resolution, resolution, 24, format);
            rt.filterMode = filterMode;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.enableRandomWrite = true;
            rt.Create();

            return rt;
        }

        private void DispatchCompute(int kernel)
        {
            computeShader.Dispatch(kernel, dispatchSize, dispatchSize, 1);
        }
        #endregion

        // #region Debug
        // public bool showDebugTextures = true;
        // public int textureToDraw = 1;
        
        // private void OnGUI()
        // {
        //     if (showDebugTextures)
        //     {
        //         int debugTextureSize = 256;

        //         GUI.Label(new Rect(0, debugTextureSize, 200, 50), "Water Map");
        //         GUI.DrawTexture(new Rect(0, 0, debugTextureSize, debugTextureSize), waterMap, ScaleMode.ScaleToFit, false);

        //         GUI.Label(new Rect(debugTextureSize, debugTextureSize, 100, 50), "Water Flow");
        //         GUI.DrawTexture(new Rect(debugTextureSize, 0, debugTextureSize, debugTextureSize), flowMap, ScaleMode.ScaleToFit, false);

        //         GUI.Label(new Rect(debugTextureSize * 2, debugTextureSize, 200, 50), "Water Velocity");
        //         GUI.DrawTexture(new Rect(debugTextureSize * 2, 0, debugTextureSize, debugTextureSize), velocityMap, ScaleMode.ScaleToFit, false);

        //         GUI.Label(new Rect(debugTextureSize * 3, debugTextureSize, 200, 50), "Soil Saturation");
        //         GUI.DrawTexture(new Rect(debugTextureSize * 3, 0, debugTextureSize, debugTextureSize), saturationMap, ScaleMode.ScaleToFit, false);
        //     }

        //     string drawingTextureName = "Drawing: ";
        //     switch (textureToDraw)
        //     {
        //         case 1:
        //             drawingTextureName += "Surface Water Flow Map";
        //             break;
        //         case 2:
        //             drawingTextureName += "Water Data Map";
        //             break;
        //         case 3:
        //             drawingTextureName += "Soil Water Saturation Map";
        //             break;
        //         case 4:
        //             drawingTextureName += "Surface Water Velocity Map";
        //             break;
        //         case 5:
        //             drawingTextureName += "Soil Use Map";
        //             break;
        //         default:
        //             drawingTextureName += "???";
        //             break;
        //     }
        //     GUI.Label(new Rect(0, 300, 200, 50), drawingTextureName);

        //     if (GUI.Button(new Rect(0, 512, 100, 50), "Reset"))
        //     {
        //         computeShader.SetTexture(kernel_reset, "worldDataMap", heightmap);
        //         computeShader.SetTexture(kernel_reset, "result", result);
        //         computeShader.SetTexture(kernel_reset, "waterMap", waterMap);
        //         computeShader.SetTexture(kernel_reset, "newWaterMap", newWaterMap);
        //         computeShader.SetTexture(kernel_reset, "saturationMap", saturationMap);
        //         computeShader.SetTexture(kernel_reset, "newSaturationMap", newSaturationMap);
        //         computeShader.SetTexture(kernel_reset, "flowMap", flowMap);
        //         computeShader.SetTexture(kernel_reset, "newFlowMap", newFlowMap);
        //         computeShader.SetTexture(kernel_reset, "velocityMap", velocityMap);
        //         computeShader.SetTexture(kernel_reset, "newVelocityMap", newVelocityMap);
        //         DispatchCompute(kernel_reset);
        //     }
        // }
        // #endregion
    }
}