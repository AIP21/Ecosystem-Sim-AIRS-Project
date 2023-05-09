using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SimDataStructure
{
    public class DataStructure : MonoBehaviour, ITickableSystem
    {
        [Header("Grids")]
        public Vector2 OverallSize;
        [Tooltip("Highest level grid is the largest grid and has the highest index")]
        public List<GridLevel> Levels;
        [SerializeField]
        private List<Grid> Grids = new List<Grid>();

        [Header("Data")]
        public List<GameObject> InitializingObjects = new List<GameObject>();
        public List<GameObject> ReadingObjects = new List<GameObject>();
        public List<GameObject> WritingObjects = new List<GameObject>();
        private List<IReadDataStructure> readingClasses = new List<IReadDataStructure>();
        private List<IWriteDataStructure> writingClasses = new List<IWriteDataStructure>();

        private Dictionary<string, AbstractGridData> cachedData = new Dictionary<string, AbstractGridData>();

        #region Interface Stuff
        public float TickPriority { get { return -1; } } // -1 because should always be highest priority
        public int TickInterval { get { return 0; } }
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
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

            Debug.Log("Data structure initialized successfully in " + st.ElapsedMilliseconds + " ms");
        }

        public void Start()
        {
            for (int i = 0; i < ReadingObjects.Count; i++)
            {
                IReadDataStructure[] readers = ReadingObjects[i].GetComponents<IReadDataStructure>();

                if (readers == null || readers.Length == 0)
                {
                    Debug.LogError("DataStructure: GameObject " + ReadingObjects[i].name + " does not have any components that implement IReadDataStructure");
                }

                readingClasses.AddRange(readers);
            }

            for (int i = 0; i < WritingObjects.Count; i++)
            {
                IWriteDataStructure[] writers = WritingObjects[i].GetComponents<IWriteDataStructure>();

                if (writers == null || writers.Length == 0)
                {
                    Debug.LogError("DataStructure: GameObject " + WritingObjects[i].name + " does not have any components that implement IWriteDataStructure");
                }

                writingClasses.AddRange(writers);
            }
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
                ISetupDataStructure initializer = InitializingObjects[i].GetComponent<ISetupDataStructure>();

                if (initializer == null)
                {
                    Debug.LogError("DataStructure: GameObject " + InitializingObjects[i].name + " does not have a component that implements ISetupDataStructure");
                }

                Dictionary<Tuple<string, int>, AbstractGridData> data = initializer.initializeData();

                foreach (KeyValuePair<Tuple<string, int>, AbstractGridData> entry in data)
                {
                    // Check if the level is valid
                    if (entry.Key.Item2 < 0 || entry.Key.Item2 >= Grids.Count)
                    {
                        Debug.LogError("DataStructure: GameObject " + InitializingObjects[i].name + " returned a data object to with an invalid level: " + entry.Key.Item2 + " (should be between 0 and " + (Grids.Count - 1) + ")");
                        continue;
                    }

                    Grids[entry.Key.Item2].SetGridData(entry.Key.Item1, entry.Value);
                }
            }
        }

        #region Systems Management
        public void BeginTick(float deltaTime)
        {
            // print("DS BeginTick");

            cachedData.Clear();

            // For every reading class, check if is tickable class and if so, if it will execute this tick.
            // If it's not a tickable class, or it is a tickable class and it will tick this tick, send the data to it
            for (int i = 0; i < readingClasses.Count; i++)
            {
                IReadDataStructure reader = readingClasses[i];

                ITickableSystem tickable = reader as ITickableSystem;

                if ((tickable != null && tickable.willTickNow)) // tickable == null || 
                {
                    // Fetch and send the data
                    sendRequestedData(reader);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            // print("DS Tick");
        }

        public void EndTick(float deltaTime)
        {
            // print("DS EndTick");

            // For every writing class, check if is tickable class and if so, if it will execute this tick.
            // If it's not a tickable class, or it is a tickable class and it will tick this tick, read the data from it
            for (int i = 0; i < writingClasses.Count; i++)
            {
                IWriteDataStructure writer = writingClasses[i];

                ITickableSystem tickable = writer as ITickableSystem;

                if ((tickable != null && tickable.willTickNow)) // tickable == null || 
                {
                    // Write the data from the writing class to the data structure
                    receiveDataFromWriter(writer);
                }
            }
        }


        // Fetches the requested data from the data structure, caches it if not already, and sends it to the reading class
        private void sendRequestedData(IReadDataStructure reader)
        {
            List<AbstractGridData> data = new List<AbstractGridData>();

            foreach (string name in reader.ReadDataNames.Keys)
            {
                int level = reader.ReadDataNames[name];

                if (cachedData.ContainsKey(name))
                {
                    // Add the cached data to the list of data to send
                    data.Add(cachedData[name]);

                    // print("A system has requested data that has already been requested. Please avoid this by making sure data is used by only one system per tick.");
                }
                else
                {
                    AbstractGridData newData = Grids[level].GetGridData(name);
                    cachedData.Add(name, newData);

                    // Add the data to the list of data to send
                    data.Add(newData);
                }
            }

            // Send the data
            reader.receiveData(data);
        }

        // Receives the new data from a writing class and writes it to the data structure
        // TODO: Make it only write ONCE, not once for every data name. It'll override it anyways so it is currently wasting writes just for them to be overwritten
        private void receiveDataFromWriter(IWriteDataStructure writer)
        {
            Dictionary<Tuple<string, int>, object> dataToWrite = writer.writeData();

            foreach (Tuple<string, int> key in dataToWrite.Keys)
            {
                string name = key.Item1;
                int level = key.Item2;

                setGridData(level, name, dataToWrite[key]);
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

        #region Cell Data Queries
        /**
        <summary>
            Get the cell data of a given name from the cell at a specific position on a specific level.
        </summary>
        **/
        public AbstractCellData GetCellData(Vector2 position, int level, string dataName)
        {
            return Grids[level]?.GetData(position, dataName);
        }

        /**
        <summary>
            Get all the cell data from the cell at a specific position on a specific level.
        </summary>
        **/
        public GenericDictionary<string, AbstractCellData> GetCellData(Vector2 position, int level)
        {
            return Grids[level]?.GetAllData(position);
        }
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
            Grids[level].SetGridData(name, data);
        }

        /**
        <summary>
            Add the given cell data to a cell at a specific position on a specific level.
        </summary>
        **/
        private void addCellData(Vector2 position, int level, string dataName, AbstractCellData data)
        {
            Grids[level].AddData(position, dataName, data);
        }

        /**
        <summary>
            Remove the cell data of a given name from a cell at a specific position on a specific level.
        </summary>
        **/
        private void removeCellData(Vector2 position, int level, string dataName)
        {
            Grids[level].RemoveData(position, dataName);
        }

        /**
        <summary>
            Remove the cell data of the given name from every cell on a specific level.
        </summary>
        **/
        private void removeCellData(int level, string dataName)
        {
            Grids[level].RemoveData(dataName);
        }
        #endregion

        private void OnDestroy()
        {
            for (int i = 0; i < Grids.Count; i++)
            {
                Grids[i].Dispose();
            }
        }
    }
}