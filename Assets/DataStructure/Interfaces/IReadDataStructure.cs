using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimDataStructure
{
    /**
        <summary>
            For a class to be able to read the data, it needs to implement the IReadDataStructure interface.
            The implementing class can read data from only ONE grid level
        </summary>
    */
    public interface IReadDataStructure
    {
        readonly int readLevel; // The grid level to receive data from
        readonly List<string> readDataNames; // The names of the data to receive

        /**
            <summary>
                Called by the data structure at the beginning of every tick to send the requested list of data to the implementing class.
                It is recommended for the implementing class to cache the received list of data for use only during the tick, to avoid memory bloat, but the received data list can also be cached for data deltas, etc.
            </summary>
        */
        void recieveData(List<AbstractGridData> sentData);
    }
}