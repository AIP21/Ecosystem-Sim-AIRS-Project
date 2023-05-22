using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Graphing;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;
using TreeGrowth.Generation;
using UnityEngine;
using UnityEngine.UIElements;

namespace TreeGrowth
{
    public class TreeManager : MonoBehaviour, ITickableSystem, IReadCellData, IWriteCellData
    {
        #region Public
        [Header("Ticking")]
        public int RandomTreeCountPerTick = 5;
        public float RandomTickRate = 0.1f;

        [Header("Initial Generation")]
        public bool CollectInitialChildTrees = true;
        public bool GenerateInitialTrees = true;
        public int InitialTreeCount = 100;
        public Bounds InitialTreeGenerationBounds = new Bounds(new Vector3(-50, 0, -50), new Vector3(50, 0, 50));
        public GameObject TestTreePrefab;

        public TreeParameters TestParameters;
        #endregion

        #region Private
        // [SerializeReference]
        private List<TreeCellData> trees = new List<TreeCellData>();
        // [SerializeReference]
        private List<TreeCellData> newTrees = new List<TreeCellData>();
        // [SerializeReference]
        private List<TreeCellData> nulledTrees = new List<TreeCellData>();

        private bool treesInitialized = false;

        private TreeParameters lastTreeParams;

        private string readWriteName = "";
        private int readWriteLevel = 0;

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

        public float TickPriority { get { return 2; } }
        public int TickInterval { get { return 20; } }
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
        public bool shouldTick { get { return this.isActiveAndEnabled; } }
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

            if ((CollectInitialChildTrees || GenerateInitialTrees) && !treesInitialized)
            {
                treesInitialized = true;

                if (CollectInitialChildTrees == true)
                    collectChildTrees();

                if (GenerateInitialTrees == true)
                    createInitialTrees();
            }

            // if (trees.Count == 0 && newTrees.Count == 0)
            // {
            //     GC.Collect();
            //     print("All trees died. Restarting with new trees and random parameters");
            //     TestParameters = lastTreeParams.Copy();
            //     createInitialTrees();
            // }
        }

        public void Tick(float deltaTime)
        {
            tickTrees();
        }

        public float averageAge = 0;
        public float averageAgeNonzero = 0;
        public int maxAge = 0;

        public void EndTick(float deltaTime)
        {
            averageAge = 0;
            averageAgeNonzero = 0;
            maxAge = int.MinValue;

            int notZeroCount = 0;

            foreach (TreeCellData tree in trees)
            {
                int age = tree.Generator.Age();

                if (age != 0)
                {
                    averageAgeNonzero += age;
                    notZeroCount++;
                }

                if (age > maxAge)
                    maxAge = age;

                averageAge += age;
            }

            averageAge = averageAge / trees.Count;
            averageAgeNonzero = averageAgeNonzero / notZeroCount;
        }

        // private void Update()
        // {
        //     List<Mesh> toDraw = combineMeshes(trees);

        //     foreach (Mesh mesh in toDraw)
        //     {
        //         Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, BarkMaterial, 0, Camera.main, 0);
        //         Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, LeafMaterial, 0, Camera.main, 1);
        //     }
        // }
        #endregion

        #region Data Structure
        public void receiveCellData(List<List<AbstractCellData>> sentData)
        {
            // All we request is the list of trees, so we can just get the first list
            List<AbstractCellData> treeData = sentData[0];

            if (treeData == null)
                return;

            // Clear the current list of trees
            trees.Clear();

            // Add all the trees to the list
            foreach (AbstractCellData data in treeData)
                trees.Add((TreeCellData)data);
        }

        public Dictionary<Tuple<string, int>, List<AbstractCellData>> writeCellDataToAdd()
        {
            Dictionary<Tuple<string, int>, List<AbstractCellData>> data = new Dictionary<Tuple<string, int>, List<AbstractCellData>>();

            // Add the data
            List<AbstractCellData> treeData = new List<AbstractCellData>();

            foreach (TreeCellData tree in this.newTrees)
            {
                treeData.Add(tree);
                trees.Add(tree);
            }

            if (treeData.Count > 0)
                data.Add(new Tuple<string, int>(readWriteName, readWriteLevel), treeData);

            return data;
        }

        public Dictionary<Tuple<string, int>, List<AbstractCellData>> writeCellDataToRemove()
        {
            Dictionary<Tuple<string, int>, List<AbstractCellData>> data = new Dictionary<Tuple<string, int>, List<AbstractCellData>>();

            // Add the data
            List<AbstractCellData> treeData = new List<AbstractCellData>();

            foreach (TreeCellData tree in this.nulledTrees)
            {
                treeData.Add(tree);
                trees.Remove(tree);
                DestroyImmediate(tree.GameObject);
            }

            if (treeData.Count > 0)
                data.Add(new Tuple<string, int>(readWriteName, readWriteLevel), treeData);

            return data;
        }
        #endregion

        #region Initialization
        private void createInitialTrees()
        {
            for (int i = 0; i < InitialTreeCount; i++)
            {
                Vector3 randPos = transform.position + new Vector3(
                    UnityEngine.Random.Range(InitialTreeGenerationBounds.min.x, InitialTreeGenerationBounds.max.x),
                    UnityEngine.Random.Range(InitialTreeGenerationBounds.min.y, InitialTreeGenerationBounds.max.y),
                    UnityEngine.Random.Range(InitialTreeGenerationBounds.min.z, InitialTreeGenerationBounds.max.z));

                RaycastHit hit;

                if (Physics.Raycast(new Ray(randPos, Vector3.down), out hit))
                {
                    randPos = hit.point;

                    GameObject testTree = Instantiate(this.TestTreePrefab, randPos, Quaternion.identity, this.transform);

                    TreeGenerator gen = testTree.GetComponent<TreeGenerator>();
                    gen.SetParameters(TestParameters.Mutate());

                    TreeCellData tree = new TreeCellData(testTree, gen, null, gen.parameters);

                    newTrees.Add(tree);
                }
            }
        }

        private void collectChildTrees()
        {
            foreach (Transform child in transform)
            {
                GameObject childGO = child.gameObject;
                TreeGenerator gen = childGO.GetComponent<TreeGenerator>();

                if (gen != null)
                {
                    // treeGen.StartCoroutine(treeGen.BuildCoroutine(TestParameters));
                    // treeGen.Build(TestParameters);
                    gen.SetParameters(TestParameters.Copy());

                    TreeCellData tree = new TreeCellData(childGO, gen, null, gen.parameters);

                    newTrees.Add(tree);
                }
            }
        }
        #endregion

        #region Tree Growth
        private void tickTrees()
        {
            if (trees.Count == 0)
                return;

            // Get the indices of the trees to tick
            int[] indices = getRandomTreeIndices(RandomTreeCountPerTick);

            foreach (int index in indices)
            {
                TreeCellData tree = trees[index];

                // if (tree.Hydration < 0)
                // {
                //     lastTreeParams = tree.TreeParameters;

                //     nulledTrees.Add(tree);
                //     print("Tree died at age " + tree.Generator.Age() + " with hydration " + tree.Hydration);
                //     continue;
                // }

                // if (tree.Hydration < tree.TreeParameters.PruneThreshold)
                //     tree.Generator.Prune(tree.TreeParameters.PrunePercentage);

                // if (tree.Hydration > tree.TreeParameters.GrowThreshold)
                tree.Generator.IterateGrowth(tree.TreeParameters);

                // if (tree.Hydration > tree.TreeParameters.ReproduceThreshold && tree.Generator.Age() > tree.TreeParameters.ReproduceAge)
                //     reproduceTree(tree);

                float used = tree.Generator.CalculateWaterUseThisTick();
                tree.Hydration -= used;

                float gained = tree.Generator.CalculateWaterAbsorptionThisTick();
                tree.Hydration += gained;

                // print("Tree " + index + " used: " + used + " gained: " + gained + " hydration: " + tree.Hydration);

                tree.TreeMesh = tree.Generator.GetMesh();
            }
        }

        // TODO: TEST OUT TREE GENERATION/EVOLUTION. it was getting stuck after a few generations on my pc

        private void reproduceTree(TreeCellData tree)
        {
            tree.Hydration -= 30;

            List<Vector3> positions = tree.Generator.GetSeedLocations(tree.TreeParameters.ReproduceAmount);

            foreach (Vector3 pos in positions)
            {
                TreeParameters newParams = tree.TreeParameters.Mutate();

                GameObject testTree = Instantiate(this.TestTreePrefab, pos, Quaternion.identity, this.transform);

                TreeGenerator gen = testTree.GetComponent<TreeGenerator>();
                gen.SetParameters(newParams); // TODO: Should this be a copy?

                newTrees.Add(new TreeCellData(testTree, gen, null, newParams));
            }
        }
        #endregion

        #region Rendering
        // private const int maxVertexCount = 65000; // Maximum vertex count per combined mesh

        // private List<Mesh> combineMeshes(List<TreeCellData> inputMeshes)
        // {
        //     List<Mesh> outputMeshes = new List<Mesh>();
        //     int vertexCount = 0;
        //     List<Mesh> meshesToCombine = new List<Mesh>();
        //     List<Vector3> meshPositions = new List<Vector3>();

        //     // Loop through all the meshes
        //     foreach (TreeCellData tree in inputMeshes)
        //     {
        //         if (tree.TreeMesh == null)
        //             continue;

        //         // If adding this mesh would exceed the max vertex count, combine the meshes
        //         if (vertexCount + tree.TreeMesh.vertexCount > maxVertexCount)
        //         {
        //             outputMeshes.Add(combineMesh(meshesToCombine, meshPositions));
        //             meshesToCombine.Clear();
        //             vertexCount = 0;
        //         }

        //         // Add the mesh to the list and update the vertex count
        //         meshesToCombine.Add(tree.TreeMesh);
        //         meshPositions.Add(tree.GameObject.transform.position);
        //         vertexCount += tree.TreeMesh.vertexCount;
        //     }

        //     // Combine any remaining meshes
        //     if (meshesToCombine.Count > 0)
        //         outputMeshes.Add(combineMesh(meshesToCombine, meshPositions));

        //     return outputMeshes;
        // }

        // private Mesh combineMesh(List<Mesh> meshes, List<Vector3> offsets)
        // {
        //     // Combine the meshes into a new mesh
        //     CombineInstance[] combineInstances = new CombineInstance[meshes.Count];
        //     for (int i = 0; i < meshes.Count; i++)
        //     {
        //         combineInstances[i].mesh = meshes[i];
        //         combineInstances[i].transform = Matrix4x4.TRS(offsets[i], Quaternion.identity, Vector3.one);
        //     }

        //     Mesh combinedMesh = new Mesh();
        //     combinedMesh.CombineMeshes(combineInstances, false, true);

        //     return combinedMesh;
        // }
        #endregion

        private int[] getRandomTreeIndices(int num)
        {
            int[] indices = new int[num];

            if (num > trees.Count)
                num = trees.Count;

            if (num == 0)
                return indices;

            if (num == trees.Count)
            {
                for (int i = 0; i < num; i++)
                    indices[i] = i;

                return indices;
            }

            for (int i = 0; i < num; i++)
            {
                if (UnityEngine.Random.value > RandomTickRate)
                    continue;

                int newNum = UnityEngine.Random.Range(0, trees.Count);

                if (indices.Contains(newNum))
                    i--;
                else
                    indices[i] = newNum;
            }

            return indices;
        }
    }
}