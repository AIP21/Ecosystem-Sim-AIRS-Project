using UnityEngine;
using System;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    // Data for whole grid (one instance per grid)
    [Serializable]
    public abstract class AbstractGridData
    {

        public AbstractGridData()
        {

        }

        public abstract void Release();
    }
}