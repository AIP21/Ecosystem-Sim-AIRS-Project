using System;
using System.Collections.Generic;
using SimDataStructure.Data;
using UnityEngine;

namespace SimDataStructure.Interfaces
{
    /**
        <summary>
            For a class to be able to read cell data, it needs to implement the IReadCellData interface.
        </summary>
    */
    public interface IReadCellData
    {
        Dictionary<string, int> ReadDataNames { get; } // The levels and names of the data to receive from the data structure

        /**
            <summary>
                Called by the data structure at the beginning of every tick to send the requested list of cell data to the implementing class.
                The receiving class should copy the data contained by the AbstractGridData objects in the list, as the data structure will reuse the same AbstractGridData objects for the next tick.


                It is recommended for the implementing class to cache the received list of data for use only during the tick, to avoid memory bloat, but the received data list can also be cached for data deltas, etc.
            </summary>
        */
        void receiveCellData(List<List<AbstractCellData>> sentData);
    }
}