using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TreeGrowth
{
    [System.Serializable]
    public record TreeParameters
    {
        private const int QUADS_PER_LEAF = 16;

        [Range(0.0f, 0.6f)]
        public float StemSize = 0.3f;

        [Range(0.0f, 0.1f)]
        public float BranchSize = 0.1f;

        [Range(0.1f, 4f)]
        public float SizeFalloff = 0.9f;

        [Range(0f, 0.5f)]
        public float Distort = 0.5f;

        [Range(0f, 1.2f)]
        public float BranchLength = 2f;

        [Range(0.1f, 0.5f)]
        public float LeafColliderSize = 0.2f;

        [Range(0f, 90f)]
        public float BranchAngle = 30f;

        [Range(0.9f, 1f)]
        public float BranchLengthFalloff = 0.9f;

        [Range(10, 1000)]
        public int Iterations = 100;

        public Node Root;

        public bool GenerateLeaves = true;

        [Range(0.2f, 1f)]
        public float LeafQuadRadius = 0.3f;

        [HideInInspector]
        public int RayCastCount;

        public int MaxChildrenPerNode = 3;

        public int MeshSubdivisions = 5;

        [Range(1, 20)]
        public int BatchSize = 5;
    }
}