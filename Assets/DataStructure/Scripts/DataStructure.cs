using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimDataStructure
{
    public class DataStructure : MonoBehaviour
    {
        [Header("Grids")]
        public Vector2 OverallSize;
        [Tooltip("Highest level grid is the largest grid and has the highest index")]
        public List<GridLevel> Levels;
        public List<Grid> Grids = new List<Grid>();

        [Header("Debug")]
        public bool DebugDraw = false;
        [Range(0f, 10f)]
        public float LevelZShift = 3f;
        public bool DebugTests = false;
        public bool TestSingleLevel = false;
        public int TestLevel = 0;
        public Vector2 TestPosition = new Vector2(0, 0);
        public List<GridCell> TestedCells = new List<GridCell>();
        public GenericDictionary<string, string> TestedCellData = new GenericDictionary<string, string>();
        public int TestedCellCount = 0;

        private Vector2 lastTestPos = new Vector2(0, 0);

        public void Awake()
        {
            // Create and populate the grids
            this.populateGrids(Levels);
            this.performDebugTests(true);
        }

        public void FixedUpdate()
        {
            if (DebugTests)
            {
                this.performDebugTests();
            }
        }

        private void performDebugTests(bool force = false)
        {
            if (!force && lastTestPos == TestPosition)
                return;

            Debug.Log("Test Position: " + TestPosition);

            lastTestPos = TestPosition;

            if (TestSingleLevel)
            {
                Grid grid = Grids[TestLevel];
                GridCell cell = grid.GetCell(TestPosition);
                TestedCells.Clear();

                if (cell != null)
                {
                    TestedCells.Add(cell);

                    Dictionary<string, AbstractGridData> data = cell.GetData();
                    TestedCellData.Clear();

                    if (data == null)
                    {
                        TestedCellData.Add("NULL", "Data is null");
                    }
                    else if (data.Count == 0){
                        TestedCellData.Add("NULL", "No data");
                    }
                    else
                    {
                        foreach (KeyValuePair<string, AbstractGridData> d in data)
                        {
                            TestedCellData.Add(d.Key, d.Value.ToString());
                        }
                    }
                }

                TestedCellCount = TestedCells.Count;
            }
            else if (Grids.Count > 0)
            {
                // Query the highest level grid for all cells that contian the test position
                // It should return a list of grid cells that contain the test position
                TestedCells = Grids[Grids.Count - 1].GetCells(TestPosition);
                TestedCellCount = TestedCells.Count;
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
                grid.ChildGrid = i == 0 ? null : Grids[i - 1];

                // If this is the highest level grid, skip
                if (i == Grids.Count - 1)
                    continue;

                // Get the grid above this one (parent grid) and set it as the parent grid
                Grid gridAbove = Grids[i + 1];
                grid.ParentGrid = gridAbove;

                // Go through each cell in this grid
                foreach (GridCell cell in grid.Cells)
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

        private void OnDrawGizmos()
        {
            if (DebugDraw)
            {
                if (Grids != null && Grids.Count > 0)
                {
                    foreach (Grid grid in Grids)
                    {
                        if (grid != null)
                            grid.DebugDrawGrid(LevelZShift);
                    }
                }

                // Draw a green box around the tested cells
                if (TestedCells != null)
                {
                    foreach (GridCell cell in TestedCells)
                    {
                        Gizmos.color = Color.green;

                        Vector3 center = new Vector3(cell.center.x, (cell.level.Level - 1) * LevelZShift, cell.center.y);

                        Gizmos.DrawLine(center + new Vector3(-cell.level.CellSize.x / 2, 0, -cell.level.CellSize.y / 2), center + new Vector3(cell.level.CellSize.x / 2, 0, -cell.level.CellSize.y / 2));
                        Gizmos.DrawLine(center + new Vector3(cell.level.CellSize.x / 2, 0, -cell.level.CellSize.y / 2), center + new Vector3(cell.level.CellSize.x / 2, 0, cell.level.CellSize.y / 2));
                        Gizmos.DrawLine(center + new Vector3(cell.level.CellSize.x / 2, 0, cell.level.CellSize.y / 2), center + new Vector3(-cell.level.CellSize.x / 2, 0, cell.level.CellSize.y / 2));
                        Gizmos.DrawLine(center + new Vector3(-cell.level.CellSize.x / 2, 0, cell.level.CellSize.y / 2), center + new Vector3(-cell.level.CellSize.x / 2, 0, -cell.level.CellSize.y / 2));
                    }
                }
            }
        }
    }

    public class Grid
    {
        public Vector2 size;
        public GridLevel gridLevel;

        public Grid ParentGrid;
        public Grid ChildGrid;

        public List<GridCell> Cells;
        public List<GridCell> ChildCells;

        public int XCellCount, YCellCount;

        public Color color;

        public Grid(Vector2 size, GridLevel gridLevel)
        {
            this.color = UnityEngine.Random.ColorHSV();

            this.size = size;
            this.gridLevel = gridLevel;

            this.Cells = new List<GridCell>();
            this.ChildCells = new List<GridCell>();

            this.XCellCount = (int)(size.x / gridLevel.CellSize.x);
            this.YCellCount = (int)(size.y / gridLevel.CellSize.y);

            populate();
        }

        private void populate()
        {
            for (int y = 0; y < YCellCount; y++)
            {
                for (int x = 0; x < XCellCount; x++)
                {
                    // Calculate the center position using size
                    Vector2 pos = new Vector2(x * gridLevel.CellSize.x + gridLevel.CellSize.x / 2, y * gridLevel.CellSize.y + gridLevel.CellSize.y / 2);

                    GridCell cell = new GridCell(this.gridLevel, pos);
                    Cells.Add(cell);

                    // Assign this cell's neighbors to the cell above and to the left
                    if (y > 0)
                    {
                        cell.neighbors[0] = Cells[Cells.Count - XCellCount - 1];
                    }
                    if (x > 0)
                    {
                        cell.neighbors[1] = Cells[Cells.Count - 2];
                    }

                    // Assign the cell above and to the left's neighbors to this cell
                    if (y > 0)
                    {
                        Cells[Cells.Count - XCellCount - 1].neighbors[2] = cell;
                    }
                    if (x > 0)
                    {
                        Cells[Cells.Count - 2].neighbors[3] = cell;
                    }
                }
            }
        }

        /**
        Return the cell that contains the query point
        **/
        public GridCell GetCell(Vector2 queryPoint)
        {
            // Calculate the index of the cell that contains the query point
            int index = (int)(queryPoint.x / gridLevel.CellSize.x) + (int)(queryPoint.y / gridLevel.CellSize.y) * XCellCount;

            if (index < 0 || index >= Cells.Count)
                return null;
            else
                return Cells[index];
        }

        /**
        Return the path of cells to the lowest-level cell that contains the query point
        **/
        public List<GridCell> GetCells(Vector2 queryPoint)
        {
            GridCell container = GetCell(queryPoint);
            if (container != null)
            {
                List<GridCell> path = container.GetLowestChild(queryPoint);
                path.Add(container);
                return path;
            }

            return null;
        }

        public void DebugDrawGrid(float levelZShift)
        {
            foreach (GridCell cell in Cells)
            {
                Gizmos.color = color;

                // Draw gizmo square using lines centered around cell center
                Vector3 center = new Vector3(cell.center.x, (this.gridLevel.Level - 1) * levelZShift, cell.center.y);

                Gizmos.DrawLine(center + new Vector3(-gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2), center + new Vector3(gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2), center + new Vector3(gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2), center + new Vector3(-gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(-gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2), center + new Vector3(-gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2));

                // For each neighbor, draw a small line to the edge of this cell
                Gizmos.color = Color.white;
                for (int i = 0; i < cell.neighbors.Length; i++)
                {
                    if (cell.neighbors[i] != null)
                    {
                        if (i == 0)
                        { // above
                            Gizmos.DrawLine(center, new Vector3(center.x, (this.gridLevel.Level - 1) * levelZShift, center.z - gridLevel.CellSize.y / 3));
                        }
                        else if (i == 1)
                        { // left
                            Gizmos.DrawLine(center, new Vector3(center.x - gridLevel.CellSize.x / 3, (this.gridLevel.Level - 1) * levelZShift, center.z));
                        }
                        else if (i == 2)
                        { // below
                            Gizmos.DrawLine(center, new Vector3(center.x, (this.gridLevel.Level - 1) * levelZShift, center.z + gridLevel.CellSize.y / 3));
                        }
                        else if (i == 3)
                        { // right
                            Gizmos.DrawLine(center, new Vector3(center.x + gridLevel.CellSize.x / 3, (this.gridLevel.Level - 1) * levelZShift, center.z));
                        }
                    }
                }

                // Draw a line to the parent cell
                if (cell.parentCell != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(center, new Vector3(cell.parentCell.center.x, (cell.parentCell.level.Level - 1) * levelZShift, cell.parentCell.center.y));
                }
            }
        }
    }

    public class GridCell
    {
        public Vector2 center;
        public Bounds bounds;
        public GridLevel level;

        public GridCell parentCell;
        public List<GridCell> childCells = new List<GridCell>();
        public GridCell[] neighbors = new GridCell[4]; // 0 = above, 1 = left, 2 = below, 3 = right

        public GenericDictionary<string, AbstractGridData> data = new GenericDictionary<string, AbstractGridData>();

        public GridCell(GridLevel level, Vector2 center)
        {
            this.center = center;
            this.level = level;
            this.bounds = new Bounds(center, level.CellSize);

            this.setupLevelData();
        }

        private void setupLevelData()
        {
            foreach (KeyValuePair<string, DataType> dataEntry in level.data)
            {
                switch (dataEntry.Value)
                {
                    case DataType.Float:
                        this.AddData(dataEntry.Key, new GridData<float>(4.20f));
                        break;
                    case DataType.Int:
                        this.AddData(dataEntry.Key, new GridData<int>(69));
                        break;
                    case DataType.Bool:
                        this.AddData(dataEntry.Key, new GridData<bool>(true));
                        break;
                    case DataType.Vector2:
                        this.AddData(dataEntry.Key, new GridData<Vector2>(new Vector2(6, 9)));
                        break;
                    case DataType.Object:
                        this.AddData(dataEntry.Key, new GridData<object>(null));
                        break;
                }
            }
        }

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

        public List<GridCell> GetLowestChild(Vector2 queryPos)
        {
            List<GridCell> path = new List<GridCell>();
            GridCell childContainer = GetChild(queryPos);

            if (childContainer != null)
            {
                path.Add(childContainer);
                path.AddRange(childContainer.GetLowestChild(queryPos));
            }
            else if (this.Contains(queryPos))
            {
                path.Add(this);
            }

            return path;
        }

        public void AddData(string name, AbstractGridData data)
        {
            this.data.Add(name, data);
        }

        public Dictionary<string, AbstractGridData> GetData()
        {
            return this.data;
        }

        public object GetData(string name)
        {
            return this.data[name];
        }
    }

    public abstract class AbstractGridData
    {
        public AbstractGridData()
        {
        }
    }

    public class GridData<T> : AbstractGridData
    {
        public T data;

        public GridData(T data) : base()
        {
            this.data = data;
        }

        public override string ToString()
        {
            return data.ToString();
        }
    }

    public enum DataType
    {
        Float,
        Int,
        Bool,
        Vector2,
        Object
    }
}