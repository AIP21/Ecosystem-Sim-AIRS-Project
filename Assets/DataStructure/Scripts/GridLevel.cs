using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimDataStructure
{
    [CreateAssetMenu(menuName = "Data Structure/GridLevel")]
    public class GridLevel : ScriptableObject
    {
        public int Level;
        public Vector2 CellSize;

        public GenericDictionary<string, SimDataStructure.CellDataType> CellDataTypes = new GenericDictionary<string, SimDataStructure.CellDataType>();
        
        public GenericDictionary<string, SimDataStructure.CellDataType> GPUData = new GenericDictionary<string, SimDataStructure.CellDataType>();
    }
}