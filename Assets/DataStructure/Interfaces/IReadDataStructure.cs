using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimDataStructure.Data;

namespace SimDataStructure.Interfaces
{
    /**
        <summary>
            For a class to be able to read the data, it needs to implement the IReadDataStructure interface.
        </summary>
    */
    public interface IReadDataStructure
    {
        Dictionary<string, int> ReadDataNames { get; } // The levels and names of the data to receive from the data structure

        /**
            <summary>
                Called by the data structure at the beginning of every tick to send the requested list of data to the implementing class.
                The receiving class should copy the data contained by the AbstractGridData objects in the list, as the data structure will reuse the same AbstractGridData objects for the next tick.


                It is recommended for the implementing class to cache the received list of data for use only during the tick, to avoid memory bloat, but the received data list can also be cached for data deltas, etc.
            </summary>
        */
        void receiveData(List<AbstractGridData> sentData);
    }
}