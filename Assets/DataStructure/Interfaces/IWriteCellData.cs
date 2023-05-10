using System;
using System.Collections.Generic;
using SimDataStructure.Data;
using UnityEngine;

namespace SimDataStructure.Interfaces
{
    /**
        <summary>
            For a class to be able to write cell data to the data structure, it needs to implement the IWriteCellData interface.
        </summary>
    */
    public interface IWriteCellData
    {
        /**
            <summary>
                Called by the data structure at the end of every tick to ADD NEW cell data to the data structure.
                This function should return a list of the data that the implementing class wants to write to the data structure.

                Return a dictionary of data to add.
            </summary>
        */
        Dictionary<Tuple<string, int>, List<AbstractCellData>> writeCellDataToAdd();
        
        /**
            <summary>
                Called by the data structure at the end of every tick to REMOVE cell data from the data structure.
                This function should return a list of the data that the implementing class wants to write to the data structure.

                Return a dictionary of data to delete.
            </summary>
        */
        Dictionary<Tuple<string, int>, List<AbstractCellData>> writeCellDataToRemove();
    }
}