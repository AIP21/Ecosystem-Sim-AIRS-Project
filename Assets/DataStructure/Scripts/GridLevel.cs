using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataStructure {
    [CreateAssetMenu(menuName = "Data Structure/GridLevel")]
    public class GridLevel : ScriptableObject
    {
        public int Level;
        public Vector2 CellSize;
        
        public Dictionary<string, DataStructure.DataType> data = new Dictionary<string, DataStructure.DataType>();
    }
}