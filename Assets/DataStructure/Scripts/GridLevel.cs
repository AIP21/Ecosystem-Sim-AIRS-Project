using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimDataStructure.Data;

namespace SimDataStructure
{
    [CreateAssetMenu(menuName = "Data Structure/GridLevel")]
    public class GridLevel : ScriptableObject
    {
        public int Level;
        public Vector2 CellSize;

        public GenericDictionary<string, CellDataType> CellDataTypes = new GenericDictionary<string, CellDataType>();
        
        public GenericDictionary<string, CellDataType> GPUData = new GenericDictionary<string, CellDataType>();

        public bool CanContainData(string dataName, CellDataType dataType)
        {
            return CellDataTypes.ContainsKey(dataName) && CellDataTypes[dataName] == dataType;
        }
    }
}