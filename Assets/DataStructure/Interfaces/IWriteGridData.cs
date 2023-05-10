using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimDataStructure.Data;

namespace SimDataStructure.Interfaces
{
    /**
        <summary>
            For a class to be able to write data to the data structure, it needs to implement the IWriteGridData interface.
        </summary>
    */
    public interface IWriteGridData
    {
        Dictionary<string, int> WriteDataNames { get; } // The levels and names of the data you are writing to the data structure

        /**
            <summary>
                Called by the data structure at the end of every tick to write data to the data structure.
                This function should return a list of the data that the implementing class wants to write to the data structure.
                The returned list of data MUST be the same length as the writeDataNames list and the index of each item in the returned list MUST correspond to its respective index in the writeDataNames list.
            </summary>
        //TODO: If this is slow, then change this to just return a list of objects
        */
        Dictionary<Tuple<string, int>, object> writeData();
    }
}