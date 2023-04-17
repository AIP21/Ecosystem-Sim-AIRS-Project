using UnityEngine;
using System;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    // Data for each cell (only inside cells)
    [Serializable]
    public abstract class AbstractCellData
    {
        public CellDataType type;

        public AbstractCellData()
        {
        }

        public CellDataType DataType()
        {
            return type;
        }

        public string DataTypeName()
        {
            return type.ToString();
        }
    }

    // [CreateAssetMenu(menuName = "Data Structure/Cell Data")]
    public class CellData<T> : AbstractCellData
    {
        public T data;

        public CellData(T data) : base()
        {
            this.data = data;

            if (typeof(T) == typeof(float))
            {
                type = CellDataType.Float;
            }
            else if (typeof(T) == typeof(int))
            {
                type = CellDataType.Int;
            }
            else if (typeof(T) == typeof(bool))
            {
                type = CellDataType.Bool;
            }
            else if (typeof(T) == typeof(Vector2))
            {
                type = CellDataType.Vector2;
            }
            else
            {
                type = CellDataType.Object;
            }
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }

    public enum CellDataType
    {
        Float,
        Int,
        Bool,
        Vector2,
        Object
    }
}