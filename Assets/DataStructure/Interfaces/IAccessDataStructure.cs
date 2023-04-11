using System;
using UnityEngine;

namespace SimDataStructure
{
    /**
        <summary>
            If a class needs to access the data structure, it should implement this interface.
            This interface allows for the class to reside on a specific level of the data structure and access the data of that level
        </summary>
    */
    public interface IAccessDataStructure
    {
        public int LevelToAccess;
        public string GridDataName;

        public void InitializeDataAccess();


    }
}