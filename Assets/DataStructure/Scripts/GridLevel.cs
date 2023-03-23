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

        public GenericDictionary<string, SimDataStructure.DataType> data = new GenericDictionary<string, SimDataStructure.DataType>();
    }
}