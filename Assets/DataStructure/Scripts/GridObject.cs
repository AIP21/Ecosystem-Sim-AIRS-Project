using System;
using NativeTrees;
using Unity.Mathematics;
using UnityEngine;

namespace DataStructure
{
    public class GridObject : MonoBehaviour
    {
        // [SerializeField] private SpriteRenderer spriteRenderer;

        public static readonly Vector2 Extents = new float2(.5f, .5f);

        public AABB2D Bounds
        {
            get
            {
                Vector2 pos = transform.position;
                return new AABB2D(pos - Extents, pos + Extents);
            }
        }

        public Color Color
        {
            get => spriteRenderer.color;
            set => spriteRenderer.color = value;
        }
    }
}