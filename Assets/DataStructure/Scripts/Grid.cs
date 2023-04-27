using UnityEngine;
using System;
using System.Collections.Generic;
using SimDataStructure.Data;

namespace SimDataStructure
{
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

        public int xCellCount, yCellCount;

        // Compute shader data
        public GenericDictionary<string, AbstractGridData> gridData = new GenericDictionary<string, AbstractGridData>();
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

        public void Release()
        {
            foreach (AbstractGridData data in gridData.Values)
            {
                data.Release();
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
        public AbstractGridData GetGridData(string dataName)
        {
            return gridData[dataName];
        }

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

        public void SetGridData(string dataName, AbstractGridData data)
        {
            gridData[dataName] = data;
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
}