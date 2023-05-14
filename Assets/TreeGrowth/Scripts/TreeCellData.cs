using System;
using System.Collections;
using System.Collections.Generic;
using SimDataStructure.Data;
using UnityEngine;
using TreeGrowth.Generation;

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
        private TreeGenerator generator;

        [SerializeField]
        [SerializeReference]
        private TreeParameters treeParameters;

        public TreeCellData(GameObject treeObj, TreeGenerator generator, Mesh mesh, MeshCollider collider, TreeParameters parameters) : base(treeObj)
        {
            this.generator = generator;
            this.treeMesh = mesh;
            this.treeCollider = collider;
            this.treeParameters = parameters;
        }

        #region Getters and Setters
        public Mesh TreeMesh
        {
            get { return treeMesh; }
            set { treeMesh = value; }
        }

        public MeshCollider TreeCollider
        {
            get { return treeCollider; }
            set { treeCollider = value; }
        }

        public TreeGenerator Generator
        {
            get { return generator; }
            set { generator = value; }
        }

        public TreeParameters TreeParameters
        {
            get { return treeParameters; }
            set { treeParameters = value; }
        }
        #endregion
    }
}