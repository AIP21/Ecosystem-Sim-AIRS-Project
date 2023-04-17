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
            The implementing class can read data from only ONE grid level
        </summary>
    */
    public interface IReadDataStructure
    {
        int ReadLevel { get; } // The grid level to receive data from
        List<string> ReadDataNames { get; } // The names of the data to receive

        /**
            <summary>
                Called by the data structure at the beginning of every tick to send the requested list of data to the implementing class.
                It is recommended for the implementing class to cache the received list of data for use only during the tick, to avoid memory bloat, but the received data list can also be cached for data deltas, etc.
            </summary>
        */
        void recieveData(List<AbstractGridData> sentData);
    }
}