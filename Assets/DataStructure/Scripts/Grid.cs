using UnityEngine;
using System;
using System.Collections.Generic;
using SimDataStructure.Data;
using Debug = UnityEngine.Debug;
using Utilities;

namespace SimDataStructure
{
    [Serializable]
    public class Grid
    {
        #region Variables
        public Vector2 size;

        private GridLevel gridLevel;
        public GridLevel GridLevel { get { return gridLevel; } }

        public Grid parentGrid;
        public Grid childGrid;

        private List<GridCell> cells;
        // private List<GridCell> childCells;

        public int xCellCount, yCellCount;

        // Compute shader data
        public GenericDictionary<string, AbstractGridData> gridData = new GenericDictionary<string, AbstractGridData>();
        #endregion

        public Grid(Vector2 size, GridLevel gridLevel)
        {
            this.size = size;
            this.gridLevel = gridLevel;

            this.cells = new List<GridCell>();
            // this.childCells = new List<GridCell>();

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
                        cell.Neighbors[0] = cells[cells.Count - xCellCount - 1];
                    }
                    if (x > 0)
                    {
                        cell.Neighbors[1] = cells[cells.Count - 2];
                    }

                    // Assign the cell above and to the left's neighbors to this cell
                    if (y > 0)
                    {
                        cells[cells.Count - xCellCount - 1].Neighbors[2] = cell;
                    }
                    if (x > 0)
                    {
                        cells[cells.Count - 2].Neighbors[3] = cell;
                    }
                }
            }
        }

        /**
        <summary>
            Move every non-static cell data to a new cell if it moved out of its original cell
        </summary>
        **/
        public void UpdateCellData()
        {
            // Move every non-static cell data to a new cell if it moved out of its original cell
            // This is done by removing the data from the old cell and adding it to the new cell

            // TODO: Maybe use a system where the data notifies the grid when it moves (on endTick, and the DS updates the grid on beginTick)?
        }

        /**
        <summary>
            Dispose all the data in this grid (only use this if you know what you're doing)
        </summary>
        **/
        public void Dispose()
        {
            foreach (AbstractGridData data in gridData.Values)
            {
                data.Dispose();
            }
        }

        #region Cell Queries
        /**
        <summary>
        Return the cell that contains the query point
        </summary>
        **/
        public GridCell GetCell(Vector3 queryPoint)
        {
            // Make sure the query point is contained in this grid, because otherwise it would overflow into the next row/column
            if (queryPoint.x < 0 || queryPoint.x > size.x || queryPoint.z < 0 || queryPoint.z > size.y)
            {
                Debug.LogError("Query point is not contained in this grid");
                return null;
            }

            // Calculate the index of the cell that contains the query point
            int index = (int)(queryPoint.x / gridLevel.CellSize.x) + (int)(queryPoint.y / gridLevel.CellSize.y) * xCellCount;

            // Check if the index is valid
            if (index >= 0 && index < cells.Count)
                return cells[index];

            Debug.LogError("Index " + index + " is out of bounds");

            return null;
        }

        /**
        <summary>
        Return the lowest-level cell that contains the query point
        </summary>
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
        <summary>
        Return the path of cells to the lowest-level cell that contains the query point
        </summary>
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
        <summary>
        Return all the cells
        </summary>
        **/
        public List<GridCell> Cells()
        {
            return cells;
        }
        #endregion

        #region Data Queries
        /**
        <summary>
            Return the grid data with the given name. 
            Returns null if the data does not exist.
        </summary>
        **/
        public AbstractGridData GetGridData(string dataName)
        {
            if (gridData.ContainsKey(dataName))
                return gridData[dataName];
            else
                return null;
        }

        /**
        <summary>
            Return all the cell data with the given name from the cell at the given position.
            Returns null if the data or cell does not exist.
        </summary>
        **/
        public List<AbstractCellData> GetCellData(Vector2 position, string dataName)
        {
            GridCell cell = GetCell(position);

            if (cell != null)
                return cell.GetCellData(dataName);

            return null;
        }

        /**
        <summary>
            Return all the cell data with the given name from every cell in this grid.
            Returns null if no cell in this grid contains the data with the given name.
        </summary>
        **/
        public List<AbstractCellData> GetCellData(string dataName)
        {
            List<AbstractCellData> data = new List<AbstractCellData>();

            foreach (GridCell cell in cells)
            {
                List<AbstractCellData> cellData = cell.GetCellData(dataName);

                if (cellData != null)
                    data.AddRange(cellData);
            }

            if (data.Count == 0)
                return null;

            return data;
        }

        /**
        <summary>
            Return ALL the cell data from the cell with the given position. 
            Returns null if no data or cell exists.
        </summary>
        **/
        public GenericDictionary<string, List<AbstractCellData>> GetAllCellData(Vector2 position)
        {
            GridCell cell = GetCell(position);

            if (cell != null)
                return cell.GetAllCellData();

            return null;
        }
        #endregion

        #region Data Management
        /**
        <summary>
            Return true if the grid level this grid belongs to can contain data with the given name and type
        </summary>
        **/
        public bool CanContainData(string dataName, Type type)
        {
            return gridLevel.CanContainData(dataName, type);
        }

        /**
        <summary>
            Set the grid data with the given name to the given data.
        </summary>
        **/
        public void SetGridData(string dataName, object data)
        {
            if (gridData.ContainsKey(dataName))
                gridData[dataName].SetData(data);
            else // TODO: Maybe add a thing here to create a new entry if it doesn't exist?
                Debug.Log("Grid data \"" + dataName + "\" does not exist.");
        }

        /**
        <summary>
            Set the grid data with the given name to the given data.
        </summary>
        **/
        public void SetGridData(string dataName, AbstractGridData data)
        {
            if (gridData.ContainsKey(dataName))
            {
                // gridData[dataName].Dispose();
                gridData[dataName].SetData(data);
            }
            else
                gridData.Add(dataName, data);
        }

        /**
        <summary>
            Set the cell data list with the given position and name to the given data list.
        </summary>
        **/
        public void SetCellData(Vector2 position, string dataName, List<AbstractCellData> data)
        {
            GridCell cell = GetCell(position);

            if (cell != null)
                cell.SetCellData(dataName, data);
        }

        /**
        <summary>
            Set the data list with the given name in every cell in this grid to the given data list.
        </summary>
        **/
        public void SetCellData(string dataName, List<AbstractCellData> data)
        {
            if (CanContainData(dataName, data.GetType()))
            {
                foreach (GridCell cell in cells)
                    cell.SetCellData(dataName, data);
            }
            else
                Debug.LogError("Data type " + data.GetType() + " is not supported by grid level " + gridLevel);
        }

        /**
        <summary>
            Add a data object to the data list with the given name of the cell that contains the data's position.
        </summary>
        **/
        public void AddCellData(string dataName, AbstractCellData data)
        {
            GridCell cell = GetCell(data.Position);

            if (cell != null)
                cell.AddCellData(dataName, data);
            else
                Debug.LogError("Trying to add cell data at a position where no cells exist: " + data.Position);
        }

        /**
        <summary>
            Remove a specific data object with the given name from the cell at the given position (if that cell contains that data object with that name).
        </summary>
        

        <returns>
            True if the data was removed, false otherwise.
        </returns>
        **/
        public bool RemoveCellData(string dataName, AbstractCellData data)
        {
            GridCell cell = GetCell(data.Position);

            if (cell != null)
                return cell.RemoveCellData(dataName, data);

            return false;
        }

        /**
        <summary>
            Remove the data with the given name from the cell at the given position (if that cell contains data of that name).
        </summary>
        

        <returns>
            True if the data was removed, false otherwise.
        </returns>
        **/
        public bool RemoveCellData(Vector2 position, string dataName)
        {
            GridCell cell = GetCell(position);

            if (cell != null)
                return cell.RemoveCellData(dataName);

            return false;
        }

        /**
        <summary>
            Remove the data with the given name from every cell in this grid (if that cell contains data of that name).
        </summary>
        **/
        public void RemoveCellData(string dataName)
        {
            foreach (GridCell cell in cells)
                cell.RemoveCellData(dataName);
        }
        #endregion
    }

    // [Serializable]
    public class GridCell
    {
        #region Public
        public Vector2 center;
        public Bounds2D bounds;
        private GridLevel level;
        [HideInInspector]
        public GridLevel Level { get { return level; } }

        // References
        private GridCell parentCell;
        [HideInInspector]
        public GridCell ParentCell { get { return parentCell; } set { parentCell = value; } }

        [HideInInspector]
        private List<GridCell> childCells = new List<GridCell>();
        [HideInInspector]
        public List<GridCell> ChildCells { get { return childCells; } }

        private GridCell[] neighbors = new GridCell[4]; // 0 = above, 1 = left, 2 = below, 3 = right
        [HideInInspector]
        public GridCell[] Neighbors { get { return neighbors; } } // 0 = above, 1 = left, 2 = below, 3 = right
        #endregion

        #region Private
        // Data
        [SerializeField]
        private GenericDictionary<string, List<AbstractCellData>> data = new GenericDictionary<string, List<AbstractCellData>>();
        #endregion

        public GridCell(GridLevel level, Vector2 center)
        {
            this.center = center;
            this.level = level;
            this.bounds = new Bounds2D(center, level.CellSize);
        }

        #region Cell Querying
        public bool Contains(Vector2 queryPos)
        {
            // if (!bounds.Contains(queryPos))
            //     return false;

            return bounds.Contains(queryPos);

            // return (queryPos.x >= center.x - Level.CellSize.x / 2 && queryPos.x <= center.x + Level.CellSize.x / 2 &&
            //         queryPos.y >= center.y - Level.CellSize.y / 2 && queryPos.y <= center.y + Level.CellSize.y / 2);
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
        /**
        <summary>
            Get the cell data of a given name from this cell
        </summary>
        **/
        public List<AbstractCellData> GetCellData(string name)
        {
            if (!this.data.ContainsKey(name))
                return null;

            return this.data[name];
        }

        /**
        <summary>
            Get all the cell data from this cell
        </summary>
        **/
        public GenericDictionary<string, List<AbstractCellData>> GetAllCellData()
        {
            return this.data;
        }

        /**
        <summary>
            Get all the cell data that moved out of this cell
        </summary>
        **/
        public Dictionary<string, List<AbstractCellData>> GetDataThatLeftCell()
        {
            Dictionary<string, List<AbstractCellData>> dataThatLeftCell = new Dictionary<string, List<AbstractCellData>>();

            foreach (KeyValuePair<string, List<AbstractCellData>> datumPair in data)
            {
                foreach (AbstractCellData datum in datumPair.Value)
                {
                    if (!this.Contains(datum.Position))
                    {
                        if (dataThatLeftCell.ContainsKey(datumPair.Key))
                            dataThatLeftCell[datumPair.Key].Add(datum);
                        else
                        {
                            List<AbstractCellData> newList = new List<AbstractCellData>();
                            newList.Add(datum);

                            dataThatLeftCell.Add(datumPair.Key, newList);
                        }
                    }
                }
            }

            return dataThatLeftCell;
        }
        #endregion

        #region Data Management
        /**
        <summary>
            Set the given list of data to this cell using the given name

            If a list of data of the given name already exists, it will be overwritten
        </summary>
        **/
        public void SetCellData(string name, List<AbstractCellData> data)
        {
            if (this.data.ContainsKey(name))
                this.data[name] = data;
            else
                this.data.Add(name, data);
        }

        /**
        <summary>
            Add the given data to this cell using the given name
        </summary>
        **/
        public void AddCellData(string name, AbstractCellData data)
        {
            if (this.data.ContainsKey(name))
                this.data[name].Add(data);
            else
            {
                List<AbstractCellData> list = new List<AbstractCellData>();
                list.Add(data);
                this.data.Add(name, list);
            }
        }

        /**
        <summary>
            Remove the data of the given name from this cell
        </summary>

        <returns>
            True if the data was removed, false otherwise
        </returns>
        **/
        public bool RemoveCellData(string name)
        {
            if (this.data.ContainsKey(name))
            {
                this.data.Remove(name);
                return true;
            }
            else
                return false;
        }

        /**
        <summary>
            Remove the given data of the given name from this cell
        </summary>

        <returns>
            True if the data was removed, false otherwise
        </returns>
        **/
        public bool RemoveCellData(string name, AbstractCellData data)
        {
            if (this.data.ContainsKey(name) && this.data[name].Contains(data))
            {
                this.data[name].Remove(data);
                return true;
            }
            else
                return false;
        }

        /**
        <summary>
            Remove all the given data from this cell
        </summary>

        <returns>
            True if every element of the given data was removed, false otherwise
        </returns>
        **/
        public bool RemoveCellData(Dictionary<string, List<AbstractCellData>> toRemove)
        {
            bool allRemoved = true;

            foreach (string name in toRemove.Keys)
            {
                foreach (AbstractCellData datum in toRemove[name])
                {
                    if (!this.RemoveCellData(name, datum))
                        allRemoved = false;
                }
            }

            return allRemoved;
        }

        public void ClearCellData()
        {
            this.data.Clear();
        }
        #endregion
    }
}