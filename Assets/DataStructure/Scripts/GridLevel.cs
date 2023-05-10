using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimDataStructure.Data;
using System;

namespace SimDataStructure
{
    [CreateAssetMenu(menuName = "Data Structure/GridLevel")]
    public class GridLevel : ScriptableObject
    {
        public int Level;
        public Vector2 CellSize;

        public GenericDictionary<string, Type> CellDataTypes = new GenericDictionary<string, Type>();

        public bool CanContainData(string dataName, Type dataType)
        {
            return CellDataTypes.ContainsKey(dataName) && CellDataTypes[dataName] == dataType;
        }
    }
}