using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;
using UnityEngine;

namespace TreeGrowth
{
    public class TreeManager : MonoBehaviour, ITickableSystem, IReadCellData, IWriteCellData
    {
        #region Public
        [SerializeReference]
        public List<TreeCellData> trees = new List<TreeCellData>();
        [SerializeReference]
        public List<TreeCellData> newTrees = new List<TreeCellData>();
        [SerializeReference]
        public List<TreeCellData> nulledTrees = new List<TreeCellData>();

        public bool first = true;
        public GameObject testTree;

        #endregion

        #region Private
        #region Interface Stuff
        // [Header("Data Structure")]
        private Dictionary<string, int> _readDataNames = new Dictionary<string, int>() {
            { "trees", 1 }, // 2
        };  // The names of the cell data this is reading from the data structure, along with its grid level
        public Dictionary<string, int> ReadDataNames { get { return _readDataNames; } }

        private Dictionary<string, int> _writeDataNames = new Dictionary<string, int>(){
            { "trees", 1 }, // 2
        };  // The names of the cell data this is writing to the data structure, along with its grid level
        public Dictionary<string, int> WriteDataNames { get { return _writeDataNames; } }

        private string readWriteName = "";
        private int readWriteLevel = 0;

        public float TickPriority { get { return 2; } }
        public int TickInterval { get { return 5; } }
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
        #endregion
        #endregion

        private void Awake()
        {
            readWriteName = WriteDataNames.Keys.ElementAt(0);
            readWriteLevel = WriteDataNames[readWriteName];
        }

        #region Ticking
        public void BeginTick(float deltaTime)
        {
            this.newTrees.Clear();
            this.nulledTrees.Clear();
        }

        public void Tick(float deltaTime)
        {
            if (first == true)
            {
                first = false;

                TreeGenerator gen = testTree.GetComponent<TreeGenerator>();
                gen.StartCoroutine(gen.BuildCoroutine());

                Mesh mesh = testTree.GetComponent<MeshFilter>().mesh;
                MeshCollider collider = testTree.GetComponent<MeshCollider>();

                TreeCellData tree = new TreeCellData(testTree, mesh, collider, null);

                newTrees.Add(tree);
            }

            // Tick each tree
            foreach (TreeCellData tree in trees)
            {
                tickTree(tree);
            }
        }

        public void EndTick(float deltaTime)
        {

        }
        #endregion

        #region Data Structure
        public void receiveCellData(List<List<AbstractCellData>> sentData)
        {
            // All we request is the list of trees, so we can just get the first list
            List<AbstractCellData> treeData = sentData[0];

            if (treeData == null)
            {
                print("Tree data is null. " + sentData.Count);
                return;
            }

            // Clear the current list of trees
            trees.Clear();

            // Add all the trees to the list
            foreach (AbstractCellData data in treeData)
            {
                trees.Add((TreeCellData)data);
            }
        }

        public Dictionary<Tuple<string, int>, List<AbstractCellData>> writeCellDataToAdd()
        {
            Dictionary<Tuple<string, int>, List<AbstractCellData>> data = new Dictionary<Tuple<string, int>, List<AbstractCellData>>();

            // Add the data
            List<AbstractCellData> treeData = new List<AbstractCellData>();

            foreach (TreeCellData tree in this.newTrees)
                treeData.Add(tree);

            data.Add(new Tuple<string, int>(readWriteName, readWriteLevel), treeData);

            return data;
        }

        public Dictionary<Tuple<string, int>, List<AbstractCellData>> writeCellDataToRemove()
        {
            Dictionary<Tuple<string, int>, List<AbstractCellData>> data = new Dictionary<Tuple<string, int>, List<AbstractCellData>>();

            // Add the data
            List<AbstractCellData> treeData = new List<AbstractCellData>();

            foreach (TreeCellData tree in this.nulledTrees)
                treeData.Add(tree);

            data.Add(new Tuple<string, int>(readWriteName, readWriteLevel), treeData);

            return data;
        }
        #endregion

        private void tickTree(TreeCellData tree)
        {

        }
    }
}