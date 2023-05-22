using System;
using System.Collections.Generic;

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
    public interface ISetupGridData
    {
        /**
            <summary>
                Pass new data to the data structure for it to store.

                This is called by the data structure only once when it is initialized.
                
                The implementing class should return a dictionary of data name to AbstractGridData objects.

                An implementing class should initialize only the data that it or its corresponding system will use.
            </summary>
        */
        Dictionary<Tuple<string, int>, object> initializeData();
    }
}