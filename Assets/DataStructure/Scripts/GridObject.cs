using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DataStructure
{
    public class GridObject : MonoBehaviour
    {
        public Transform trans
        {
            get; set
            {

            }
        }

        public Vector3 position
        {
            get
            {
                return trans.position;
            }

            set
            {
                trans.position = value;
            }
        }

        public float x
        {
            get
            {
                return trans.position.x;
            }

            set
            {
                trans.position = new Vector3(value, trans.position.y, trans.position.z);
            }
        }
        public float y
        {
            public get
            {
                return trans.position.y;
            }

            public set
            {
                trans.position = new Vector3(trans.position.x, value, trans.position.z);
            }
        }
        public float z
        {
            public get
            {
                return trans.position.z;
            }

            public set
            {
                trans.position = new Vector3(trans.position.x, trans.position.y, value);
            }
        }

        public static readonly Vector2 Extents = new Vector2(.5f, .5f);
        public List<int> gridIndex { get; set; }

        public Awake()
        {
            this.trans = transform;
            this.gridIndex = new List<int>();
        }

        public Bounds Bounds
        {
            get
            {
                Vector2 pos = transform.position;
                return new Bounds(pos - Extents, pos + Extents);
            }
        }

        public Color Color
        {
            get => spriteRenderer.color;
            set => spriteRenderer.color = value;
        }


    }
}