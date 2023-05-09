using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimDataStructure.Data;

namespace SimDataStructure.Interfaces
{
    /**
        <summary>
            Initialize the data structure with data.
            This is called by the data structure when it is initialized.
            
            The implementing class should return a dictionary of data name to AbstractGridData objects.

            An implementing class should initialize only the data that it or its corresponding system will use.
        </summary>
    */
    public interface ISetupDataStructure
    {
        /**
            <summary>
                Pass new data to the data structure for it to store.

                This is called by the data structure only once when it is initialized.
                
                The implementing class should return a dictionary of data name to AbstractGridData objects.

                An implementing class should initialize only the data that it or its corresponding system will use.
            </summary>
        */
        Dictionary<Tuple<string, int>, AbstractGridData> initializeData();
    }
}