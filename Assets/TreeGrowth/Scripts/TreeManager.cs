using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;
using UnityEngine;

namespace TreeGrowth
{
    public class TreeManager : MonoBehaviour, ITickableSystem, IReadDataStructure, IWriteDataStructure, ISetupDataStructure
    {

        #region Private


        #region Interface Stuff
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

        public float TickPriority { get { return 2; } }
        public int TickInterval { get { return 5; } }
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
        #endregion
        #endregion

        private void Awake()
        {

        }

        #region Ticking
        public void BeginTick(float deltaTime)
        {

        }

        public void Tick(float deltaTime)
        {

        }

        public void EndTick(float deltaTime)
        {

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
                        // data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RFloat, FilterMode.Bilinear));
                        break;
                    case "waterFlow":
                        // data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear));
                        break;
                    case "waterVelocity":
                        // data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RGFloat, FilterMode.Bilinear));
                        break;
                    case "soilSaturation":
                        // data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RFloat, FilterMode.Bilinear));
                        break;
                    case "soilUse":
                        // data.Add(new Tuple<string, int>(name, ReadDataNames[name]), new TextureGridData(resolution, RenderTextureFormat.RFloat, FilterMode.Bilinear));
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
                        // dat.GetData(waterMap);

                        break;
                    case "waterFlow":
                        // print("Receiving waterFlow map");
                        // dat.GetData(flowMap);

                        break;
                    case "waterVelocity":
                        // print("Receiving waterVelocity map");
                        // dat.GetData(velocityMap);

                        break;
                    case "soilSaturation":
                        // print("Receiving soilSaturation map");
                        // dat.GetData(saturationMap);

                        break;
                    case "soilUse":
                        // print("Receiving soilUse map");
                        // dat.GetData(soilUseMap);

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
                        // toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newWaterMap);

                        break;
                    case "waterFlow":
                        // print("Writing waterFlow map");
                        // toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newFlowMap);

                        break;
                    case "waterVelocity":
                        // print("Writing waterVelocity map");
                        // toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newVelocityMap);

                        break;
                    case "soilSaturation":
                        // print("Writing soilSaturation map");
                        // toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newSaturationMap);

                        break;
                    case "soilUse":
                        // print("Writing soilUse map");
                        // toWrite.Add(new Tuple<string, int>(name, WriteDataNames[name]), newSoilUseMap);

                        break;
                }
            }

            return toWrite;
        }
        #endregion
    }
}