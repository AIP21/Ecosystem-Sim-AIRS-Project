using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using SimDataStructure.Interfaces;
using SimDataStructure.Data;
using Managers.Interfaces;

namespace SimDataStructure
{
    public class DataStructure : MonoBehaviour, ITickableSystem
    {
        [Header("Grids")]
        public Vector2 OverallSize;
        [Tooltip("Highest level grid is the largest grid and has the highest index")]
        public List<GridLevel> Levels;
        public List<Grid> Grids = new List<Grid>();

        public List<IReadDataStructure> ReadingClasses = new List<IReadDataStructure>();
        public List<IWriteDataStructure> WritingClasses = new List<IWriteDataStructure>();

        private Dictionary<string, AbstractGridData> cachedData = new Dictionary<string, AbstractGridData>();

        public List<IReadDataStructure> willTick = new List<IReadDataStructure>();

        #region Interface Stuff
        public int TickPriority { get { return 0; } }
        public int TickInterval { get { return 1; } }
        public int lastTick { get; set; }
        public bool willTickNow { get; set; }
        #endregion

        public void Awake()
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            // Create and populate the grids
            this.populateGrids(Levels);

            st.Stop();

            Debug.Log("Data structure initialized successfully in " + st.ElapsedMilliseconds + " ms");
        }

        /*
        Create a single grid for each level
        Go through each grid (from bottom to top) and check which cell in the grid above it is within bounds of
        Set its parent cell to that container cell
        Add it to the container cell's children list
        */
        private void populateGrids(List<GridLevel> levels)
        {
            // Create grids
            for (int i = 0; i < levels.Count; i++)
            {
                Grids.Add(new Grid(OverallSize, levels[i]));
            }

            // Populate grids from lowest level to highest level
            for (int i = 0; i < Grids.Count; i++)
            {
                Grid grid = Grids[i];

                // Set the child grid of the grid below this one
                grid.childGrid = i == 0 ? null : Grids[i - 1];

                // If this is the highest level grid, skip
                if (i == Grids.Count - 1)
                    continue;

                // Get the grid above this one (parent grid) and set it as the parent grid
                Grid gridAbove = Grids[i + 1];
                grid.parentGrid = gridAbove;

                // Go through each cell in this grid
                foreach (GridCell cell in grid.Cells())
                {
                    // Get the cell in the parent that contains the current cell's center position
                    GridCell containerCell = gridAbove.GetCell(cell.center);

                    if (containerCell == null)
                    {
                        print("Container cell is null");
                        continue;
                    }

                    // Assign the parent cell to the container cell
                    cell.parentCell = containerCell;
                    containerCell.childCells.Add(cell);
                }
            }

            // Initialize all grids
            for (int i = 0; i < Grids.Count; i++)
            {
                Grids[i].Initialize();
            }
        }

        #region Systems Management
        public void BeginTick()
        {
            cachedData.Clear();
            willTick.Clear();

            // For every reading class, check if is tickabkle class and if so, if it will execute this tick.
            // If it's not a tickable class, or it is a tickable class and it will tick this tick, send the data to it
            for (int i = 0; i < ReadingClasses.Count; i++)
            {
                IReadDataStructure readingClass = ReadingClasses[i];

                bool isTickable = readingClass is ITickableSystem;

                if (!(isTickable) || (isTickable && ((ITickableSystem)readingClass).willTickNow))
                {
                    if (isTickable)
                        willTick.Add(readingClass);
                        
                    // Fetch and send the data
                    readingClass.recieveData(getRequestedData(readingClass.ReadLevel, readingClass.ReadDataNames));
                }
            }
        }

        // Returns the cached data requested by a reading class, and caches it if it's not already cached
        private List<AbstractGridData> getRequestedData(int level, List<string> dataNames)
        {
            List<AbstractGridData> data = new List<AbstractGridData>();

            for (int e = 0; e < dataNames.Count; e++)
            {
                string dataName = dataNames[e];
                if (cachedData.ContainsKey(dataName))
                    data.Add(cachedData[dataName]);
                else
                {
                    AbstractGridData newData = Grids[level].GetData(dataName);
                    data.Add(newData);
                    cachedData.Add(dataName, newData);
                }
            }

            return data;
        }

        public void Tick()
        {
            for (int i = 0; i < WritingClasses.Count; i++)
            {
                WritingClasses[i].Tick();
            }
        }

        public void EndTick()
        {
            for (int i = 0; i < WritingClasses.Count; i++)
            {
                WritingClasses[i].EndTick();
            }
        }
        #endregion

        #region Cell Queries
        public GridCell GetCell(Vector2 queryPoint, int level)
        {
            return Grids[level].GetCell(queryPoint);
        }

        public GridCell GetLowestCell(Vector2 queryPoint, int startingLevel)
        {
            return Grids[startingLevel].GetLowestCell(queryPoint);
        }

        public GridCell GetCells(Vector2 queryPoint, int startingLevel)
        {
            return Grids[startingLevel].GetLowestCell(queryPoint);
        }

        public List<GridCell> GetPathToLowestCell(Vector2 queryPoint, int startingLevel)
        {
            return Grids[startingLevel].GetPathToLowestCell(queryPoint);
        }
        #endregion

        #region Data Queries
        public AbstractCellData GetData(Vector2 position, int level, string dataName)
        {
            return Grids[level]?.GetData(position, dataName);
        }

        public Dictionary<string, AbstractCellData> GetDataOfType(Vector2 position, int level, CellDataType type)
        {
            return Grids[level]?.GetDataOfType(position, type);
        }

        public GenericDictionary<string, AbstractCellData> GetAllData(Vector2 position, int level)
        {
            return Grids[level]?.GetAllData(position);
        }
        #endregion

        #region Data Management
        public void SetData(Vector2 position, int level, string dataName, AbstractCellData data)
        {
            Grids[level].SetData(position, dataName, data);
        }

        public void SetData(int level, string dataName, AbstractCellData data)
        {
            Grids[level].SetData(dataName, data);
        }

        /** USE AT YOUR OWN RISK, YOU SHOULD NOT (UNDER ANY CIRCUMSTANCES) BE ADDING DATA TO INDIVIDUAL CELLS. IF YOU NEED TO ADD DATA, JUST ADD IT TO THE ENTIRE GRID OR DEFINE IT IN THE GRID LEVEL. **/
        public void AddData(Vector2 position, int level, string dataName, CellDataType dataType, AbstractCellData data, bool ignoreChecks = false)
        {
            if (!ignoreChecks)
                Debug.LogWarning("You are adding data to an individual cell in the data structure. This is (REALLY!) not recommended and may cause errors. Use at your own risk! If you need to add data, just add it to the entire grid or define it in the grid level.");

            Grids[level].AddData(position, dataName, dataType, data, ignoreChecks);
        }

        // This is ok though
        public void AddData(int level, string dataName, CellDataType dataType, AbstractCellData data, bool ignoreChecks = false)
        {
            Grids[level].AddData(dataName, dataType, data, ignoreChecks);
        }

        /** USE AT YOUR OWN RISK, YOU SHOULD NOT (UNDER ANY CIRCUMSTANCES) BE REMOVING DATA FROM INDIVIDUAL CELLS. IF YOU WANT TO REMOVE DATA JUST REMOVE IT FROM THE ENTIRE GRID. **/
        public void RemoveData(Vector2 position, int level, string dataName, bool ignoreChecks = false)
        {
            if (!ignoreChecks)
                Debug.LogWarning("You are removing data from an individual cell in the data structure. This is (REALLY!) not recommended and may cause errors. Use at your own risk! If you want to remove data, remove it from the entire grid.");

            Grids[level].RemoveData(position, dataName, ignoreChecks);
        }

        // This is ok though
        public void RemoveData(int level, string dataName)
        {
            Grids[level].RemoveData(dataName);
        }
        #endregion
    }
}