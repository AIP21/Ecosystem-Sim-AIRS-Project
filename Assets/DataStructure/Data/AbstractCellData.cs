using UnityEngine;
using System;
using System.Collections.Generic;

namespace SimDataStructure.Data
{
    // Data for each cell (only inside cells)
    [Serializable]
    public abstract class AbstractCellData
    {
        /**
        <summary>
            Whether this cell data is static.

            A static cell data means that it's gameObject does not move around, and so would always in the same cell.
        </summary>
        **/
        private bool isStatic;

        private GameObject gameObject;

        private Transform transform;

        public AbstractCellData(GameObject gameObject)
        {
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
        }

        #region Getters and Setters
        /**
        <summary>
            Whether this cell data is static.

            A static cell data means that it's gameObject does not move around, and so would always in the same cell.
        </summary>
        **/
        public bool IsStatic { get { return isStatic; } }

        public GameObject GameObject { get { return gameObject; } }

        public Transform Transform { get { return transform; } }

        public Vector3 Position { get { return transform.position; } }
        #endregion
    }

    // // [CreateAssetMenu(menuName = "Data Structure/Cell Data")]
    // public class CellData<T> : AbstractCellData
    // {
    //     public CellDataType type;

    //     public T data;

    //     public CellData(T data) : base()
    //     {
    //         this.data = data;

    //         if (typeof(T) == typeof(float))
    //         {
    //             type = CellDataType.Float;
    //         }
    //         else if (typeof(T) == typeof(int))
    //         {
    //             type = CellDataType.Int;
    //         }
    //         else if (typeof(T) == typeof(bool))
    //         {
    //             type = CellDataType.Bool;
    //         }
    //         else if (typeof(T) == typeof(Vector2))
    //         {
    //             type = CellDataType.Vector2;
    //         }
    //         else
    //         {
    //             type = CellDataType.Object;
    //         }
    //     }

    //     public override string ToString()
    //     {
    //         return data.ToString();
    //     }

    //     public CellDataType DataType()
    //     {
    //         return type;
    //     }

    //     public string DataTypeName()
    //     {
    //         return type.ToString();
    //     }
    // }

    // public enum CellDataType
    // {
    //     Float,
    //     Int,
    //     Bool,
    //     Vector2,
    //     Object
    // }
}