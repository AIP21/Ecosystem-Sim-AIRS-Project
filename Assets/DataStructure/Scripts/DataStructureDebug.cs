using System.Linq;
using System.Dynamic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using SimDataStructure.Interfaces;
using SimDataStructure.Data;

namespace SimDataStructure
{
    public class DataStructureDebug : MonoBehaviour
    {
        [Header("References")]
        public DataStructure ds;

        private Dictionary<Grid, Color> gridColors = new Dictionary<Grid, Color>();

        [Header("Drawing")]
        public bool DrawAll = false;
        public bool DrawRelationshipLines = false;
        public bool DrawParentLines = false;
        [Range(0f, 100f)]
        public float LevelZShift = 3f;

        #region Test Variables
        [Header("Tests")]
        public bool DoQueryTest = true;
        public int QueryTestCount = 1000;
        // public bool DoAddDataTest = true;
        // public int AddDataTestCount = 1000;
        // public bool DoRemoveDataTest = true;
        // public int RemoveDataTestCount = 1000;
        // public bool DoModifyDataTest = true;
        // public int ModifyDataTestCount = 1000;

        [Tooltip("Set to -1 to test random levels")]
        public int TestLevel = -1;

        [Header("Single cell query (used to get data contained by single cell)")]
        public bool QuerySingleCellTest = false;
        public int QuerySingleCellLevel = 0;
        public Vector2 QuerySingleCellPosition = new Vector2(0, 0);

        [Header("Results from single cell test")]
        public List<GridCell> TestedCells = new List<GridCell>();
        public GenericDictionary<string, List<String>> TestedCellData = new GenericDictionary<string, List<String>>();
        public int TestedCellCount = 0;

        private Vector2 lastTestPos = new Vector2(0, 0);
        #endregion

        public void Start()
        {
            foreach (Grid grid in ds.Grids)
            {
                gridColors.Add(grid, UnityEngine.Random.ColorHSV());
            }
        }

        public void FixedUpdate()
        {
            if (DoQueryTest)
            {
                DoQueryTest = false;
                performQueryTest(QueryTestCount);
            }

            if (QuerySingleCellTest && QuerySingleCellPosition != lastTestPos)
            {
                lastTestPos = QuerySingleCellPosition;
                performDebugQuery(QuerySingleCellPosition, QuerySingleCellLevel);
            }
        }

        #region Tests
        public void performQueryTest(int count)
        {
            Debug.Log("Performing " + count + " random queries on " + (TestLevel == -1 ? "random level" : ("level " + TestLevel)) + "...");

            Stopwatch st = new Stopwatch();
            st.Start();

            for (int i = 0; i < count; i++)
            {
                Vector2 testPos = new Vector2(UnityEngine.Random.Range(0, ds.OverallSize.x), UnityEngine.Random.Range(0, ds.OverallSize.y));
                performDebugQuery(testPos, TestLevel == -1 ? UnityEngine.Random.Range(0, ds.Grids.Count - 1) : TestLevel, false);
            }
            st.Stop();

            Debug.Log(count + " random queries successfully completed in " + st.Elapsed.TotalMilliseconds + " ms");
        }

        private void performDebugQuery(Vector2 queryPos, int level = -1, bool fetchData = true)
        {
            if (level != -1)
            {
                if (!fetchData)
                {
                    GridCell cell2 = ds.Grids[level].GetCell(queryPos);
                    TestedCells.Clear();

                    if (cell2 != null)
                    {
                        TestedCells.Add(cell2);
                    }

                    return;
                }

                Grid grid = ds.Grids[level];
                GridCell cell = grid.GetCell(queryPos);
                TestedCells.Clear();

                if (cell != null)
                {
                    TestedCells.Add(cell);

                    GenericDictionary<string, List<AbstractCellData>> data = cell.GetAllCellData();
                    TestedCellData.Clear();

                    if (data == null)
                    {
                        TestedCellData.Add("NULL", new List<string>() { "Data is null" });
                    }
                    else if (data.Count == 0)
                    {
                        TestedCellData.Add("NULL", new List<string>() { "No data" });
                    }
                    else
                    {
                        foreach (KeyValuePair<string, List<AbstractCellData>> d in data)
                        {
                            List<string> names = new List<string>();

                            foreach (AbstractCellData datum in d.Value)
                                names.Add(datum.ToString());

                            TestedCellData.Add(d.Key, names);
                        }
                    }
                }

                TestedCellCount = TestedCells.Count;
            }
            else if (ds.Grids.Count > 0)
            {
                // Query the highest level grid for all cells that contain the test position
                // It should return a list of grid cells that contain the test position
                TestedCells = ds.Grids[ds.Grids.Count - 1].GetPathToLowestCell(queryPos);
                TestedCellCount = TestedCells.Count;
            }
        }

        /*
        public void performAddDataTest(int count)
        {
            Debug.Log("Performing " + count + " random data additions on " + (TestLevel == -1 ? "random level" : ("level " + TestLevel)) + "...");

            Stopwatch st = new Stopwatch();
            st.Start();

            for (int i = 0; i < count; i++)
            {
                Vector2 testPos = new Vector2(UnityEngine.Random.Range(0, ds.OverallSize.x), UnityEngine.Random.Range(0, ds.OverallSize.y));
                int testLevel = TestLevel == -1 ? UnityEngine.Random.Range(0, ds.Grids.Count - 1) : TestLevel;

                AbstractCellData randData = randomData();

                // ds.AddData(testPos, testLevel, Guid.NewGuid().ToString(), randData.type, randData, true);
            }

            st.Stop();

            Debug.Log(count + " random data additions successfully completed in " + st.ElapsedMilliseconds + " ms");
        }

        public void performRemoveDataTest(int count)
        {
            Debug.Log("Performing " + count + " random data removals on " + (TestLevel == -1 ? "random level" : ("level " + TestLevel)) + "...");

            Stopwatch st = new Stopwatch();
            st.Start();

            for (int i = 0; i < count; i++)
            {
                Vector2 testPos = new Vector2(UnityEngine.Random.Range(0, ds.OverallSize.x), UnityEngine.Random.Range(0, ds.OverallSize.y));
                int testLevel = TestLevel == -1 ? UnityEngine.Random.Range(0, ds.Grids.Count - 1) : TestLevel;

                AbstractCellData randData = randomData();

                GenericDictionary<string, CellDataType> possibleData = ds.Grids[testLevel].GridLevel.CellDataTypes;
                if (possibleData.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, possibleData.Count - 1);
                    string randomKey = possibleData.Keys.ToArray()[randomIndex];

                    // ds.RemoveData(testPos, testLevel, randomKey, true);
                }
                else
                {
                    Debug.Log("No data to remove");
                }
            }

            st.Stop();

            Debug.Log(count + " random data removals successfully completed in " + st.ElapsedMilliseconds + " ms");
        }

        public void performModifyDataTest(int count)
        {
            Debug.Log("Performing " + count + " random data modifications on " + (TestLevel == -1 ? "random level" : ("level " + TestLevel)) + "...");

            Stopwatch st = new Stopwatch();
            st.Start();

            for (int i = 0; i < count; i++)
            {
                Vector2 testPos = new Vector2(UnityEngine.Random.Range(0, ds.OverallSize.x), UnityEngine.Random.Range(0, ds.OverallSize.y));
                int testLevel = TestLevel == -1 ? UnityEngine.Random.Range(0, ds.Grids.Count - 1) : TestLevel;


                GenericDictionary<string, CellDataType> possibleData = ds.Grids[testLevel].GridLevel.CellDataTypes;
                if (possibleData.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, possibleData.Count - 1);
                    string randomKey = possibleData.Keys.ToArray()[randomIndex];
                    AbstractCellData randData = randomData(possibleData[randomKey]);

                    // ds.SetData(testPos, testLevel, randomKey, randData);
                }
                else
                {
                    // Debug.Log("No data to modify");
                }
            }

            st.Stop();

            Debug.Log(count + " random data modifications successfully completed in " + st.ElapsedMilliseconds + " ms");
        }
        */
        #endregion

        private void OnDrawGizmos()
        {
            if (DrawAll)
            {
                if (ds.Grids != null && ds.Grids.Count > 0)
                {
                    foreach (Grid grid in ds.Grids)
                    {
                        if (grid != null)
                            debugDrawGrid(grid, LevelZShift);
                    }
                }

                // Draw a green box around the tested cells
                if (TestedCells != null)
                {
                    foreach (GridCell cell in TestedCells)
                    {
                        Gizmos.color = Color.green;

                        Vector3 center = new Vector3(cell.center.x, (cell.Level.Level - 1) * LevelZShift, cell.center.y);

                        Gizmos.DrawLine(center + new Vector3(-cell.Level.CellSize.x / 2, 0, -cell.Level.CellSize.y / 2), center + new Vector3(cell.Level.CellSize.x / 2, 0, -cell.Level.CellSize.y / 2));
                        Gizmos.DrawLine(center + new Vector3(cell.Level.CellSize.x / 2, 0, -cell.Level.CellSize.y / 2), center + new Vector3(cell.Level.CellSize.x / 2, 0, cell.Level.CellSize.y / 2));
                        Gizmos.DrawLine(center + new Vector3(cell.Level.CellSize.x / 2, 0, cell.Level.CellSize.y / 2), center + new Vector3(-cell.Level.CellSize.x / 2, 0, cell.Level.CellSize.y / 2));
                        Gizmos.DrawLine(center + new Vector3(-cell.Level.CellSize.x / 2, 0, cell.Level.CellSize.y / 2), center + new Vector3(-cell.Level.CellSize.x / 2, 0, -cell.Level.CellSize.y / 2));
                    }
                }
            }
        }

        public void debugDrawGrid(Grid toDraw, float levelZShift)
        {
            foreach (GridCell cell in toDraw.Cells())
            {
                Gizmos.color = Color.white;

                // Draw gizmo square using lines centered around cell center
                Vector3 center = new Vector3(cell.center.x, (toDraw.GridLevel.Level - 1) * levelZShift, cell.center.y);

                Gizmos.DrawLine(center + new Vector3(-toDraw.GridLevel.CellSize.x / 2, 0, -toDraw.GridLevel.CellSize.y / 2), center + new Vector3(toDraw.GridLevel.CellSize.x / 2, 0, -toDraw.GridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(toDraw.GridLevel.CellSize.x / 2, 0, -toDraw.GridLevel.CellSize.y / 2), center + new Vector3(toDraw.GridLevel.CellSize.x / 2, 0, toDraw.GridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(toDraw.GridLevel.CellSize.x / 2, 0, toDraw.GridLevel.CellSize.y / 2), center + new Vector3(-toDraw.GridLevel.CellSize.x / 2, 0, toDraw.GridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(-toDraw.GridLevel.CellSize.x / 2, 0, toDraw.GridLevel.CellSize.y / 2), center + new Vector3(-toDraw.GridLevel.CellSize.x / 2, 0, -toDraw.GridLevel.CellSize.y / 2));

                if (DrawRelationshipLines)
                {
                    // For each neighbor, draw a small line to the edge of this cell
                    Gizmos.color = Color.white;
                    for (int i = 0; i < cell.Neighbors.Length; i++)
                    {
                        if (cell.Neighbors[i] != null)
                        {
                            if (i == 0)
                            { // above
                                Gizmos.DrawLine(center, new Vector3(center.x, (toDraw.GridLevel.Level - 1) * levelZShift, center.z - toDraw.GridLevel.CellSize.y / 3));
                            }
                            else if (i == 1)
                            { // left
                                Gizmos.DrawLine(center, new Vector3(center.x - toDraw.GridLevel.CellSize.x / 3, (toDraw.GridLevel.Level - 1) * levelZShift, center.z));
                            }
                            else if (i == 2)
                            { // below
                                Gizmos.DrawLine(center, new Vector3(center.x, (toDraw.GridLevel.Level - 1) * levelZShift, center.z + toDraw.GridLevel.CellSize.y / 3));
                            }
                            else if (i == 3)
                            { // right
                                Gizmos.DrawLine(center, new Vector3(center.x + toDraw.GridLevel.CellSize.x / 3, (toDraw.GridLevel.Level - 1) * levelZShift, center.z));
                            }
                        }
                    }
                }

                if (DrawParentLines)
                {
                    // Draw a line to the parent cell
                    if (cell.ParentCell != null)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(center, new Vector3(cell.ParentCell.center.x, (cell.ParentCell.Level.Level - 1) * levelZShift, cell.ParentCell.center.y));
                    }
                }
            }
        }
    }
}