using System;
using System.Collections;
using System.Collections.Generic;
using SimDataStructure.Data;
using UnityEngine;

namespace TreeGrowth
{
    [System.Serializable]
    public class TreeCellData : AbstractCellData
    {
        [SerializeField]
        private Mesh treeMesh;
        [SerializeField]
        private MeshCollider treeCollider;

        [SerializeField]
        [SerializeReference]
        private TreeParameters treeParameters;

        public TreeCellData(GameObject treeObj, Mesh mesh, MeshCollider collider, TreeParameters parameters) : base(treeObj)
        {
            treeMesh = mesh;
            treeCollider = collider;
            treeParameters = parameters;
        }
    }
}