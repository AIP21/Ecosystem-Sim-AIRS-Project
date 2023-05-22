using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Utilities;

namespace SimDataStructure
{
    public class DataStructure : MonoBehaviour, ITickableSystem
    {
        #region Public
        [Header("Grids")]
        public Vector2 OverallSize;

        [Tooltip("Highest level grid is the largest grid and has the highest index")]
        public List<GridLevel> Levels;

        [SerializeField]
        private List<Grid> grids = new List<Grid>();
        public List<Grid> Grids { get { return grids; } }

        [Header("Data")]
        public List<GameObject> InitializingObjects = new List<GameObject>();
        public List<GameObject> ReadingObjects = new List<GameObject>();
        public List<GameObject> WritingObjects = new List<GameObject>();
        #endregion

        #region Private
        private List<IReadGridData> gridReadingClasses = new List<IReadGridData>();
        private List<IWriteGridData> gridWritingClasses = new List<IWriteGridData>();
        private List<IReadCellData> cellReadingClasses = new List<IReadCellData>();
        private List<IWriteCellData> cellWritingClasses = new List<IWriteCellData>();

        private Dictionary<string, AbstractGridData> cachedGridData = new Dictionary<string, AbstractGridData>();
        private Dictionary<string, List<AbstractCellData>> cachedCellData = new Dictionary<string, List<AbstractCellData>>();
        #endregion

        #region Interface Stuff
        public float TickPriority { get { return -1; } } // -1 because should always be highest priority
        public int TickInterval { get { return 0; } }
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
        public bool shouldTick { get { return true; } }
        #endregion

        #region Debug
        [Header("Debug")]
        public bool CalculateDebugInfo = false;
        public int StatHistory = 50;

        [Space(10)]
        public float gridReadsPerTick = 0;
        public float gridWritesPerTick = 0;
        public float gridActivityPerTick = 0;
        [Space(5)]
        public float cellReadsPerTick = 0;
        public float cellWritesPerTick = 0;
        public float cellActivityPerTick = 0;

        [Space(10)]
        public float gridReadTimePerTick = 0;
        public float gridWriteTimePerTick = 0;
        [Space(5)]
        public float cellReadTimePerTick = 0;
        public float cellWriteTimePerTick = 0;

        private List<float> _gridReadsPerTick = new List<float>();
        private List<float> _gridWritesPerTick = new List<float>();
        private List<float> _cellReadsPerTick = new List<float>();
        private List<float> _cellWritesPerTick = new List<float>();

        private List<float> _gridReadTimePerTick = new List<float>();
        private List<float> _gridWriteTimePerTick = new List<float>();

        private List<float> _cellReadTimePerTick = new List<float>();
        private List<float> _cellWriteTimePerTick = new List<float>();
        #endregion

        public void Awake()
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            // Create and populate the grids
            this.populateGrids(Levels);

            // Initialize all the grid data
            this.initializeGridData();

            st.Stop();

            Debug.Log("Data structure initialized successfully. Took: " + st.ElapsedMilliseconds + "ms");
        }

        public void Start()
        {
            print("Data Structure started scanning reader and writer gameObjects");

            Stopwatch st = new Stopwatch();
            st.Start();

            for (int i = 0; i < ReadingObjects.Count; i++)
            {
                IReadGridData[] gridReaders = ReadingObjects[i].GetComponents<IReadGridData>();
                gridReadingClasses.AddRange(gridReaders);

                IReadCellData[] cellReaders = ReadingObjects[i].GetComponents<IReadCellData>();
                cellReadingClasses.AddRange(cellReaders);

                if ((gridReaders == null || gridReaders.Length == 0) && (cellReaders == null || cellReaders.Length == 0))
                {
                    Debug.LogError("DataStructure: GameObject " + ReadingObjects[i].name + " does not have any components that implement IReadGridData or IReadCellData");
                }
            }

            for (int i = 0; i < WritingObjects.Count; i++)
            {
                IWriteGridData[] gridWriters = WritingObjects[i].GetComponents<IWriteGridData>();
                gridWritingClasses.AddRange(gridWriters);

                IWriteCellData[] cellWriters = WritingObjects[i].GetComponents<IWriteCellData>();
                cellWritingClasses.AddRange(cellWriters);

                if ((gridWriters == null || gridWriters.Length == 0) && (cellWriters == null || cellWriters.Length == 0))
                {
                    Debug.LogError("DataStructure: GameObject " + ReadingObjects[i].name + " does not have any components that implement IWriteGridData or IWriteCellData");
                }
            }

            st.Stop();
            print("Data Structure finished scanning reader and writer gameObjects. Took " + st.ElapsedMilliseconds + "ms");
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
                grids.Add(new Grid(OverallSize, levels[i]));
            }

            // Populate grids from lowest level to highest level
            for (int i = 0; i < grids.Count; i++)
            {
                Grid grid = grids[i];

                // Set the child grid of the grid below this one
                grid.childGrid = i == 0 ? null : grids[i - 1];

                // If this is the highest level grid, skip
                if (i == grids.Count - 1)
                    continue;

                // Get the grid above this one (parent grid) and set it as the parent grid
                Grid gridAbove = grids[i + 1];
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
                    cell.ParentCell = containerCell;
                    containerCell.ChildCells.Add(cell);
                }
            }

            // // Initialize all grids
            // for (int i = 0; i < Grids.Count; i++)
            // {
            //     Grids[i].Initialize();
            // }
        }

        private void initializeGridData()
        {
            for (int i = 0; i < InitializingObjects.Count; i++)
            {
                ISetupGridData initializer = InitializingObjects[i].GetComponent<ISetupGridData>();

                if (initializer == null)
                {
                    Debug.LogError("DataStructure: GameObject " + InitializingObjects[i].name + " does not have a component that implements ISetupGridData");
                }

                Dictionary<Tuple<string, int>, object> data = initializer.initializeData();

                foreach (KeyValuePair<Tuple<string, int>, object> entry in data)
                {
                    // Check if the level is valid
                    if (entry.Key.Item2 < 0 || entry.Key.Item2 >= grids.Count)
                    {
                        Debug.LogError("DataStructure: GameObject " + InitializingObjects[i].name + " returned a data object to with an invalid level: " + entry.Key.Item2 + " (should be between 0 and " + (grids.Count - 1) + ")");
                        continue;
                    }

                    if (entry.Value is AbstractGridData){
                        grids[entry.Key.Item2].SetGridData(entry.Key.Item1, (AbstractGridData)entry.Value);
                    } else {
                        Debug.LogError("DataStructure: GameObject " + InitializingObjects[i].name + " returned a data object that does not inherit from AbstractGridData");
                    }
                }
            }
        }

        #region Systems Management
        public void BeginTick(float deltaTime)
        {
            // print("DS BeginTick");

            cachedGridData.Clear();
            cachedCellData.Clear();

            int gridReadCount = 0;
            int cellReadCount = 0;

            Stopwatch st = null;

            if (CalculateDebugInfo)
            {
                st = new Stopwatch();
                st.Start();
            }

            // For every grid reading class, check if is tickable class and if so, if it will execute this tick.
            // If it's not a tickable class, or it is a tickable class and it will tick this tick, send the data to it
            for (int i = 0; i < gridReadingClasses.Count; i++)
            {
                IReadGridData reader = gridReadingClasses[i];

                ITickableSystem tickable = reader as ITickableSystem;

                if (tickable == null || (tickable != null && tickable.willTickNow))
                {
                    // Fetch and send the data
                    int reads = sendRequestedGridData(reader);

                    if (CalculateDebugInfo)
                        gridReadCount += reads;
                }
            }

            if (CalculateDebugInfo)
            {
                st.Stop();

                Utils.AddToAverageList<float>(_gridReadTimePerTick, (float)st.Elapsed.TotalMilliseconds, StatHistory);

                if (gridReadCount > 0)
                    Utils.AddToAverageList<float>(_gridReadsPerTick, gridReadCount, StatHistory);

                st.Reset();
                st.Start();
            }

            // For every cell reading class, check if is tickable class and if so, if it will execute this tick.
            // If it's not a tickable class, or it is a tickable class and it will tick this tick, send the data to it
            for (int i = 0; i < cellReadingClasses.Count; i++)
            {
                IReadCellData reader = cellReadingClasses[i];

                ITickableSystem tickable = reader as ITickableSystem;

                if (tickable == null || (tickable != null && tickable.willTickNow))
                {
                    // Fetch and send the data
                    int reads = sendRequestedCellData(reader);

                    if (CalculateDebugInfo)
                        cellReadCount += reads;
                }
            }

            if (CalculateDebugInfo)
            {
                st.Stop();

                Utils.AddToAverageList<float>(_cellReadTimePerTick, (float)st.Elapsed.TotalMilliseconds, StatHistory);

                if (cellReadCount > 0)
                    Utils.AddToAverageList<float>(_cellReadsPerTick, cellReadCount, StatHistory);
            }
        }

        public void Tick(float deltaTime)
        {
            // print("DS Tick");
        }

        public void EndTick(float deltaTime)
        {
            // print("DS EndTick");

            // TODO: Move every non-static cell data to a new cell if it moved
            // Update grid cells (move every non-static cell data to a new cell if it moved)
            // foreach (Grid grid in Grids){
            //     grid.UpdateCellData();
            // }

            int gridWriteCount = 0;
            int cellWriteCount = 0;

            Stopwatch st = null;

            if (CalculateDebugInfo)
            {
                st = new Stopwatch();
                st.Start();
            }

            // For every grid writing class, check if is tickable class and if so, if it will execute this tick.
            // If it's not a tickable class, or it is a tickable class and it will tick this tick, read the data from it
            for (int i = 0; i < gridWritingClasses.Count; i++)
            {
                IWriteGridData writer = gridWritingClasses[i];

                ITickableSystem tickable = writer as ITickableSystem;

                if (tickable == null || (tickable != null && tickable.willTickNow))
                {
                    // Write the data from the writing class to the data structure
                    int writes = receiveGridDataFromWriter(writer);

                    if (CalculateDebugInfo)
                        gridWriteCount += writes;
                }
            }

            if (CalculateDebugInfo)
            {
                st.Stop();

                Utils.AddToAverageList<float>(_gridWriteTimePerTick, (float)st.Elapsed.TotalMilliseconds, StatHistory);

                st.Reset();
                st.Start();
            }

            // For every cell writing class, check if is tickable class and if so, if it will execute this tick.
            // If it's not a tickable class, or it is a tickable class and it will tick this tick, read the data from it
            for (int i = 0; i < cellWritingClasses.Count; i++)
            {
                IWriteCellData writer = cellWritingClasses[i];

                ITickableSystem tickable = writer as ITickableSystem;

                if (tickable == null || (tickable != null && tickable.willTickNow))
                {
                    // Write the data from the writing class to the data structure
                    int writes = receiveCellDataFromWriter(writer);

                    if (CalculateDebugInfo)
                        cellWriteCount += writes;
                }
            }

            if (CalculateDebugInfo)
            {
                st.Stop();

                Utils.AddToAverageList<float>(_cellWriteTimePerTick, (float)st.Elapsed.TotalMilliseconds, StatHistory);

                if (gridWriteCount > 0)
                    Utils.AddToAverageList<float>(_gridWritesPerTick, gridWriteCount, StatHistory);
                if (cellWriteCount > 0)
                    Utils.AddToAverageList<float>(_cellWritesPerTick, cellWriteCount, StatHistory);

                gridReadsPerTick = Utils.Average(_gridReadsPerTick);
                cellReadsPerTick = Utils.Average(_cellReadsPerTick);

                gridWritesPerTick = Utils.Average(_gridWritesPerTick);
                cellWritesPerTick = Utils.Average(_cellWritesPerTick);

                gridReadTimePerTick = Utils.Average(_gridReadTimePerTick);
                cellReadTimePerTick = Utils.Average(_cellReadTimePerTick);

                gridWriteTimePerTick = Utils.Average(_gridWriteTimePerTick);
                cellWriteTimePerTick = Utils.Average(_cellWriteTimePerTick);

                gridActivityPerTick = gridReadsPerTick + gridWritesPerTick;
                cellActivityPerTick = cellReadsPerTick + cellWritesPerTick;
            }
        }
        #endregion

        #region IO for Grid Data
        // Fetches the requested data from the data structure, caches it if not already, and sends it to the reading class
        private int sendRequestedGridData(IReadGridData reader)
        {
            List<AbstractGridData> data = new List<AbstractGridData>();

            int reads = 0;

            foreach (string name in reader.ReadDataNames.Keys)
            {
                int level = reader.ReadDataNames[name];

                if (cachedGridData.ContainsKey(name))
                {
                    // Add the cached data to the list of data to send
                    data.Add(cachedGridData[name]);

                    // print("A system has requested data that has already been requested. Please avoid this by making sure data is used by only one system per tick.");
                }
                else
                {
                    AbstractGridData newData = grids[level].GetGridData(name);
                    cachedGridData.Add(name, newData);

                    // Add the data to the list of data to send
                    data.Add(newData);
                }

                reads++;
            }

            // Send the data
            reader.readData(data);

            return reads;
        }

        // Receives the new data from a writing class and writes it to the data structure
        // TODO: Make it only write ONCE, not once for every data name. It'll override it anyways so it is currently wasting writes just for them to be overwritten

        // TODO: Change it to not pass a tuple and just the level to write it on, int is the key and the value is a list of data
        private int receiveGridDataFromWriter(IWriteGridData writer)
        {
            Dictionary<Tuple<string, int>, object> dataToWrite = writer.writeData();

            int writes = 0;

            foreach (Tuple<string, int> key in dataToWrite.Keys)
            {
                string name = key.Item1;
                int level = key.Item2;

                setGridData(level, name, dataToWrite[key]);

                writes++;
            }

            return writes;
        }
        #endregion

        #region IO for Cell Data
        // Fetches the requested cell data from the data structure, caches it if not already, and sends it to the reading class
        private int sendRequestedCellData(IReadCellData reader)
        {
            // List<Dictionary<int, List<AbstractCellData>>> data = new List<Dictionary<int, List<AbstractCellData>>>(); // (CURRENTLY NOT USED!) This is for if we want to pass cell index for some reason
            List<List<AbstractCellData>> data = new List<List<AbstractCellData>>();

            int reads = 0;

            foreach (string name in reader.ReadDataNames.Keys)
            {
                int level = reader.ReadDataNames[name];

                if (cachedCellData.ContainsKey(name))
                {
                    // Add the cached data to the list of data to send
                    data.Add(cachedCellData[name]);

                    if (CalculateDebugInfo && cachedCellData[name] != null)
                        reads += cachedCellData[name].Count;
                }
                else
                {
                    List<AbstractCellData> cellData = grids[level].GetCellData(name);
                    // print("Cell data: " + cellData.Count);

                    cachedCellData.Add(name, cellData);

                    // Add the data to the list of data to send
                    data.Add(cellData);

                    if (CalculateDebugInfo && cellData != null)
                        reads += cellData.Count;
                }
            }

            // Send the data
            reader.receiveCellData(data);

            return reads;
        }

        // Receives the new cell data from a writing class and writes it to the data structure
        private int receiveCellDataFromWriter(IWriteCellData writer)
        {
            Dictionary<Tuple<string, int>, List<AbstractCellData>> dataToAdd = writer.writeCellDataToAdd();

            int writes = 0;

            foreach (Tuple<string, int> dataPointer in dataToAdd.Keys)
            {
                string name = dataPointer.Item1;
                int level = dataPointer.Item2;

                addCellData(level, name, dataToAdd[dataPointer]);

                if (CalculateDebugInfo && dataToAdd[dataPointer] != null)
                    writes += dataToAdd[dataPointer].Count;
            }

            Dictionary<Tuple<string, int>, List<AbstractCellData>> dataToRemove = writer.writeCellDataToRemove();

            foreach (Tuple<string, int> dataPointer in dataToRemove.Keys)
            {
                string name = dataPointer.Item1;
                int level = dataPointer.Item2;

                removeCellData(level, name, dataToRemove[dataPointer]);

                if (CalculateDebugInfo && dataToRemove[dataPointer] != null)
                    writes += dataToRemove[dataPointer].Count;
            }

            return writes;
        }
        #endregion

        #region Cell Queries (no longer used. Also should not be used)
        // public GridCell GetCell(Vector2 queryPoint, int level)
        // {
        //     return Grids[level].GetCell(queryPoint);
        // }

        // public GridCell GetLowestCell(Vector2 queryPoint, int startingLevel)
        // {
        //     return Grids[startingLevel].GetLowestCell(queryPoint);
        // }

        // public GridCell GetCells(Vector2 queryPoint, int startingLevel)
        // {
        //     return Grids[startingLevel].GetLowestCell(queryPoint);
        // }

        // public List<GridCell> GetPathToLowestCell(Vector2 queryPoint, int startingLevel)
        // {
        //     return Grids[startingLevel].GetPathToLowestCell(queryPoint);
        // }
        #endregion

        #region Cell Data Queries (no longer used. Also should not be used)
        // /**
        // <summary>
        //     Get the cell data of a given name from the cell at a specific position on a specific level.
        // </summary>
        // **/
        // public List<AbstractCellData> GetCellData(Vector2 position, int level, string dataName)
        // {
        //     return Grids[level]?.GetCellData(position, dataName);
        // }

        // /**
        // <summary>
        //     Get all the cell data from the cell at a specific position on a specific level.
        // </summary>
        // **/
        // public GenericDictionary<string, List<AbstractCellData>> GetCellData(Vector2 position, int level)
        // {
        //     return Grids[level]?.GetAllCellData(position);
        // }
        #endregion

        #region Data Management
        /**
        <summary>
            Set the grid data of a given name on a given level to the given data.
        </summary>

        <param name="level">The level to set the data on.</param>
        <param name="name">The name of the data you are setting.</param>
        <param name="data">The data to set to the grid. It is an object, but should be of the type AbstractGridData.</param>
        **/
        private void setGridData(int level, string name, object data)
        {
            grids[level].SetGridData(name, data);
        }

        /**
        <summary>
            Add the given list of cell data and its position to a cell at that position on a specific level.
        </summary>
        **/
        private void addCellData(int level, string dataName, List<AbstractCellData> data)
        {
            Grid grid = grids[level];

            foreach (AbstractCellData datum in data)
                grid.AddCellData(dataName, datum);
        }

        /**
        <summary>
            Remove the data in the given dictionary of cell data and its position from a cell at that position on a specific level.
        </summary>
        **/
        private void removeCellData(int level, string dataName, List<AbstractCellData> data)
        {
            Grid grid = grids[level];

            foreach (AbstractCellData datum in data)
                grid.RemoveCellData(dataName, datum);
        }

        /**
        <summary>
            Remove the cell data of the given name from every cell on a specific level.
        </summary>
        **/
        private void removeCellData(int level, string dataName)
        {
            grids[level].RemoveCellData(dataName);
        }
        #endregion

        private void OnDestroy()
        {
            foreach (Grid grid in grids)
                grid.Dispose();
        }
    }
}