using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace SimDataStructure
{
    public class DataStructure : MonoBehaviour
    {
        [Header("Grids")]
        public Vector2 OverallSize;
        [Tooltip("Highest level grid is the largest grid and has the highest index")]
        public List<GridLevel> Levels;
        public List<Grid> Grids = new List<Grid>();

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
        }

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

    #region Grid
    public class Grid
    {
        #region Variables
        public Vector2 size;

        private GridLevel gridLevel;
        public GridLevel GridLevel { get { return gridLevel; } }

        public Grid parentGrid;
        public Grid childGrid;

        private List<GridCell> cells;
        private List<GridCell> childCells;

        // public List<GridCell> Cells { get { return cells; } }
        // public List<GridCell> ChildCells { get { return childCells; } }

        public int xCellCount, yCellCount;

        // Data
        private AbstractGridData data;
        #endregion

        public Grid(Vector2 size, GridLevel gridLevel)
        {
            this.size = size;
            this.gridLevel = gridLevel;

            this.cells = new List<GridCell>();
            this.childCells = new List<GridCell>();

            this.xCellCount = (int)(size.x / gridLevel.CellSize.x);
            this.yCellCount = (int)(size.y / gridLevel.CellSize.y);

            populate();
        }

        private void populate()
        {
            for (int y = 0; y < yCellCount; y++)
            {
                for (int x = 0; x < xCellCount; x++)
                {
                    // Calculate the center position using size
                    Vector2 pos = new Vector2(x * gridLevel.CellSize.x + gridLevel.CellSize.x / 2, y * gridLevel.CellSize.y + gridLevel.CellSize.y / 2);

                    GridCell cell = new GridCell(this.gridLevel, pos);
                    cells.Add(cell);

                    // Assign this cell's neighbors to the cell above and to the left
                    if (y > 0)
                    {
                        cell.neighbors[0] = cells[cells.Count - xCellCount - 1];
                    }
                    if (x > 0)
                    {
                        cell.neighbors[1] = cells[cells.Count - 2];
                    }

                    // Assign the cell above and to the left's neighbors to this cell
                    if (y > 0)
                    {
                        cells[cells.Count - xCellCount - 1].neighbors[2] = cell;
                    }
                    if (x > 0)
                    {
                        cells[cells.Count - 2].neighbors[3] = cell;
                    }
                }
            }
        }

        #region Cell Queries
        /**
        Return the cell that contains the query point
        **/
        public GridCell GetCell(Vector2 queryPoint)
        {
            // Calculate the index of the cell that contains the query point
            int index = (int)(queryPoint.x / gridLevel.CellSize.x) + (int)(queryPoint.y / gridLevel.CellSize.y) * xCellCount;

            if (index < 0 || index >= cells.Count)
                return null;
            else
                return cells[index];
        }

        /**
        Return the lowest-level cell that contains the query point
        **/
        public GridCell GetLowestCell(Vector2 queryPoint)
        {
            GridCell container = GetCell(queryPoint);
            if (container != null)
            {
                return container.GetLowestChild(queryPoint);
            }

            return null;
        }

        /**
        Return the path of cells to the lowest-level cell that contains the query point
        **/
        public List<GridCell> GetPathToLowestCell(Vector2 queryPoint)
        {
            GridCell container = GetCell(queryPoint);
            if (container != null)
            {
                List<GridCell> path = container.GetPathToLowestChild(queryPoint);
                path.Add(container);
                return path;
            }

            return null;
        }

        /**
        Return all the cells
        **/
        public List<GridCell> Cells()
        {
            return cells;
        }
        #endregion

        #region Data Queries
        public AbstractCellData GetData(Vector2 position, string dataName)
        {
            GridCell cell = GetCell(position);
            if (cell != null)
            {
                return cell.GetData(dataName);
            }

            return null;
        }

        public Dictionary<string, AbstractCellData> GetDataOfType(Vector2 position, CellDataType type)
        {
            GridCell cell = GetCell(position);
            if (cell != null)
            {
                return cell.GetDataOfType(type);
            }

            return null;
        }

        public GenericDictionary<string, AbstractCellData> GetAllData(Vector2 position)
        {
            GridCell cell = GetCell(position);
            if (cell != null)
            {
                return cell.GetAllData();
            }

            return null;
        }
        #endregion

        #region Data Management
        public bool CanContainData(string dataName, CellDataType type)
        {
            return gridLevel.CanContainData(dataName, type);
        }

        public void SetData(Vector2 position, string dataName, AbstractCellData data)
        {
            GridCell cell = GetCell(position);
            if (cell != null)
            {
                cell.SetData(dataName, data);
            }
        }

        public void SetData(string dataName, AbstractCellData data)
        {
            foreach (GridCell cell in cells)
            {
                cell.SetData(dataName, data);
            }
        }

        /** USE AT YOUR OWN RISK, YOU SHOULD NOT (UNDER ANY CIRCUMSTANCES) BE ADDING DATA TO INDIVIDUAL CELLS. IF YOU NEED TO ADD DATA, JUST ADD IT TO THE ENTIRE GRID OR DEFINE IT IN THE GRID LEVEL. **/
        public void AddData(Vector2 position, string dataName, CellDataType dataType, AbstractCellData data, bool ignoreChecks = false)
        {
            if (!ignoreChecks)
                Debug.LogWarning("You are adding data to an individual cell in the data structure. This is (REALLY!) not recommended and may cause errors. Use at your own risk! If you need to add data, just add it to the entire grid or define it in the grid level.");

            if (ignoreChecks || CanContainData(dataName, dataType))
            {
                GridCell cell = GetCell(position);
                if (cell != null)
                {
                    cell.AddData(dataName, data);
                }
            }
            else
            {
                Debug.LogError("Data type " + data.GetType() + " is not supported by grid level " + gridLevel);
            }
        }

        // This is ok though
        public void AddData(string dataName, CellDataType dataType, AbstractCellData data, bool ignoreChecks = false)
        {
            if (ignoreChecks || CanContainData(dataName, dataType))
            {
                foreach (GridCell cell in cells)
                {
                    cell.AddData(dataName, data);
                }
            }
            else
            {
                Debug.LogError("Data type " + data.GetType() + " is not supported by grid level " + gridLevel);
            }
        }

        /** USE AT YOUR OWN RISK, YOU SHOULD NOT (UNDER ANY CIRCUMSTANCES) BE REMOVING DATA FROM INDIVIDUAL CELLS. IF YOU WANT TO REMOVE DATA JUST REMOVE IT FROM THE ENTIRE GRID. **/
        public void RemoveData(Vector2 position, string dataName, bool ignoreChecks = false)
        {
            if (!ignoreChecks)
                Debug.LogWarning("You are removing data from an individual cell in the data structure. This is (REALLY!) not recommended and may cause errors. Use at your own risk! If you want to remove data, remove it from the entire grid.");

            GridCell cell = GetCell(position);
            if (cell != null)
            {
                cell.RemoveData(dataName);
            }
        }

        // This is ok though
        public void RemoveData(string dataName)
        {
            foreach (GridCell cell in cells)
            {
                cell.RemoveData(dataName);
            }
        }
        #endregion
    }

    public class GridCell
    {
        #region Public
        public Vector2 center;
        public Bounds bounds;
        public GridLevel level;

        // References
        public GridCell parentCell;
        public List<GridCell> childCells = new List<GridCell>();
        public GridCell[] neighbors = new GridCell[4]; // 0 = above, 1 = left, 2 = below, 3 = right
        #endregion

        #region Private
        // Data
        private GenericDictionary<string, AbstractCellData> data = new GenericDictionary<string, AbstractCellData>();
        #endregion

        public GridCell(GridLevel level, Vector2 center)
        {
            this.center = center;
            this.level = level;
            this.bounds = new Bounds(center, level.CellSize);

            this.setupLevelData();
        }

        private void setupLevelData()
        {
            foreach (KeyValuePair<string, CellDataType> dataEntry in level.CellDataTypes)
            {
                switch (dataEntry.Value)
                {
                    case CellDataType.Float:
                        this.AddData(dataEntry.Key, new CellData<float>(4.20f));
                        break;
                    case CellDataType.Int:
                        this.AddData(dataEntry.Key, new CellData<int>(69));
                        break;
                    case CellDataType.Bool:
                        this.AddData(dataEntry.Key, new CellData<bool>(true));
                        break;
                    case CellDataType.Vector2:
                        this.AddData(dataEntry.Key, new CellData<Vector2>(new Vector2(6, 9)));
                        break;
                    case CellDataType.Object:
                        this.AddData(dataEntry.Key, new CellData<object>(null));
                        break;
                }
            }
        }

        #region Cell Querying
        public bool Contains(Vector2 queryPos)
        {
            if (!bounds.Contains(queryPos))
            {
                return false;
            }

            return (queryPos.x >= center.x - level.CellSize.x / 2 && queryPos.x <= center.x + level.CellSize.x / 2 &&
                    queryPos.y >= center.y - level.CellSize.y / 2 && queryPos.y <= center.y + level.CellSize.y / 2);
        }

        public GridCell GetChild(Vector2 queryPos)
        {
            foreach (GridCell cell in childCells)
            {
                if (cell.Contains(queryPos))
                {
                    return cell;
                }
            }

            return null;
        }

        public GridCell GetLowestChild(Vector2 queryPos)
        {
            GridCell childContainer = GetChild(queryPos);

            if (childContainer != null)
            {
                return childContainer.GetLowestChild(queryPos);
            }
            else if (this.Contains(queryPos))
            {
                return this;
            }

            return null;
        }

        public List<GridCell> GetPathToLowestChild(Vector2 queryPos)
        {
            List<GridCell> path = new List<GridCell>();
            GridCell childContainer = GetChild(queryPos);

            if (childContainer != null)
            {
                path.Add(childContainer);
                path.AddRange(childContainer.GetPathToLowestChild(queryPos));
            }
            else if (this.Contains(queryPos))
            {
                path.Add(this);
            }

            return path;
        }
        #endregion

        #region Data Querying
        public AbstractCellData GetData(string name)
        {
            return this.data[name];
        }

        public Dictionary<string, AbstractCellData> GetDataOfType(CellDataType type)
        {
            Dictionary<string, AbstractCellData> dataOfType = new Dictionary<string, AbstractCellData>();

            foreach (KeyValuePair<string, AbstractCellData> dataEntry in this.data)
            {
                if (dataEntry.Value.type == type)
                {
                    dataOfType.Add(dataEntry.Key, dataEntry.Value);
                }
            }

            return dataOfType;
        }

        public GenericDictionary<string, AbstractCellData> GetAllData()
        {
            return this.data;
        }
        #endregion

        #region Data Management
        public void SetData(string name, AbstractCellData data)
        {
            if (this.data.ContainsKey(name))
                this.data[name] = data;
        }

        public void AddData(string name, AbstractCellData data)
        {
            // Debug.LogWarning("Data was added to a single grid cell at position " + this.center + " on level " + this.level);
            this.data.Add(name, data);
        }

        public void RemoveData(string name)
        {
            // Debug.LogWarning("Data was removed from a single grid cell at position " + this.center + " on level " + this.level);
            this.data.Remove(name);
        }
        #endregion
    }
    #endregion

    #region Data
    // Data for each cell (only inside cells)
    public abstract class AbstractCellData
    {
        public CellDataType type;

        public AbstractCellData()
        {
        }

        public CellDataType DataType()
        {
            return type;
        }

        public string DataTypeName()
        {
            return type.ToString();
        }
    }

    public class CellData<T> : AbstractCellData
    {
        public T data;

        public CellData(T data) : base()
        {
            this.data = data;

            if (typeof(T) == typeof(float))
            {
                type = CellDataType.Float;
            }
            else if (typeof(T) == typeof(int))
            {
                type = CellDataType.Int;
            }
            else if (typeof(T) == typeof(bool))
            {
                type = CellDataType.Bool;
            }
            else if (typeof(T) == typeof(Vector2))
            {
                type = CellDataType.Vector2;
            }
            else
            {
                type = CellDataType.Object;
            }
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }

    public enum CellDataType
    {
        Float,
        Int,
        Bool,
        Vector2,
        Object
    }

    // Data for whole grid (one instance per grid)
    // This class will contain a reference to a compute shader that will be a grid shader that computes some data for each cell
    // This class will contain a method to update the shader
    // This class will contain a method to access the data of the shader
    // This class will contain a method to supply input data to the shader
    public abstract class AbstractGridData
    {
        public AbstractGridData()
        {
        }

    }

    public class ShaderGridData : AbstractGridData
    {
        // This class will contain a reference to a compute shader that will be a grid shader that computes some data for each cell
        // This class will contain a method to update the shader
        // This class will contain a method to access the data of the shader
        // This class will contain a method to supply input data to the shader


    }
    #endregion
}