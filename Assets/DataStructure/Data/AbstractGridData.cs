using UnityEngine;
using System;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    // Data for whole grid (one instance per grid).
    // This should be treated as IMMUTABLE
    [Serializable]
    public abstract class AbstractGridData
    {

        public AbstractGridData()
        {

        }

        /**
        <summary>
            Copies the data from this AbstractGridData to the given object.
        </summary>
        **/
        public abstract void GetData(object target);

        /**
        <summary>
            Copies the data from the given object to this AbstractGridData.
        </summary>
        **/
        public abstract void SetData(object source);

        /**
        <summary>
            Dispose this data. (Dispose of any buffers, textures, etc from memory)
            This should ALWAYS be called before this object is destroyed or discarded. (Although, this should not be commonly used because you want to keep the same data object at runtime)
        </summary>
        **/
        public abstract void Dispose();
    }
}