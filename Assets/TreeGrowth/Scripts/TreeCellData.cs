using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;
using UnityEngine;

namespace TreeGrowth {
    public class TreeCellData : CellData<Object> {
        public TreeCellData(Object data) : base(data) {
        }
    }
}