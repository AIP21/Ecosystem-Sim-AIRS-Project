using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructure
{
    public class DataStructure : MonoBehaviour
    {
        [Header("Grids")]
        public Vector2 OverallSize;
        [Tooltip("Highest level grid is the largest grid and has the highest index")]
        public List<GridLevel> Levels;
        public List<Grid> Grids = new List<Grid>();

        [Header("Debug")]
        [Range(0f, 10f)]
        public float LevelZShift = 3f;

        public void Start(){
            // Create and populate the grids
            populateGrids(Levels);
        }

        public void FixedUpdate()
        {
            
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
                    GridCell containerCell = gridAbove.GetCellAt(cell.center);

                    if (containerCell == null) {
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
            if (Grids != null && Grids.Count > 0){
                foreach (Grid grid in Grids)
                {                    
                    if (grid != null)
                        grid.DebugDrawGrid(LevelZShift);
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

            createCells();
        }

        private void createCells() {
            for (int y = 0; y < YCellCount; y++)
            {
                for (int x = 0; x < XCellCount; x++)
                {
                    // Calculate the center position using size
                    Vector2 pos = new Vector2(x * gridLevel.CellSize.x + gridLevel.CellSize.x / 2, y * gridLevel.CellSize.y + gridLevel.CellSize.y / 2);
                    
                    GridCell cell = new GridCell(this.gridLevel, pos);
                    Cells.Add(cell);

                    // Assign this cell's neighbors to the cell above and to the left
                    if (y > 0){
                        cell.neighbors[0] = Cells[Cells.Count - XCellCount - 1];
                    }
                    if (x > 0) {
                        cell.neighbors[1] = Cells[Cells.Count - 2];
                    }
                    
                    // Assign the cell above and to the left's neighbors to this cell
                    if (y > 0){
                        Cells[Cells.Count - XCellCount - 1].neighbors[2] = cell;
                    }
                    if (x > 0){
                        Cells[Cells.Count - 2].neighbors[3] = cell;
                    }
                }
            }
        }

        /**
        Return the cell that contains the query point
        **/
        public GridCell GetCellAt(Vector2 queryPoint) {
            // Calculate the index of the cell that contains the query point
            int index = (int)(queryPoint.x / gridLevel.CellSize.x) + (int)(queryPoint.y / gridLevel.CellSize.y) * XCellCount;
            
            if (index < 0 || index >= Cells.Count)
                return null;
            else
                return Cells[index];
        }

        public void DebugDrawGrid(float levelZShift) {
            foreach (GridCell cell in Cells) {
                Gizmos.color = color;
                
                // Draw gizmo square using lines centered around cell center
                Vector3 center = new Vector3(cell.center.x, (this.gridLevel.Level - 1) * levelZShift, cell.center.y);
                
                Gizmos.DrawLine(center + new Vector3(-gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2), center + new Vector3(gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2), center + new Vector3(gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2), center + new Vector3(-gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2));
                Gizmos.DrawLine(center + new Vector3(-gridLevel.CellSize.x / 2, 0, gridLevel.CellSize.y / 2), center + new Vector3(-gridLevel.CellSize.x / 2, 0, -gridLevel.CellSize.y / 2));

                // For each neighbor, draw a small line to the edge of this cell
                Gizmos.color = Color.white;
                for (int i = 0; i < cell.neighbors.Length; i++) {
                    if (cell.neighbors[i] != null) {
                        if (i == 0) { // above
                            Gizmos.DrawLine(center, new Vector3(center.x, (this.gridLevel.Level - 1) * levelZShift, center.z - gridLevel.CellSize.y / 3));
                        } else if (i == 1) { // left
                            Gizmos.DrawLine(center, new Vector3(center.x - gridLevel.CellSize.x / 3, (this.gridLevel.Level - 1) * levelZShift, center.z));
                        } else if (i == 2) { // below
                            Gizmos.DrawLine(center, new Vector3(center.x, (this.gridLevel.Level - 1) * levelZShift, center.z + gridLevel.CellSize.y / 3));
                        } else if (i == 3) { // right
                            Gizmos.DrawLine(center, new Vector3(center.x + gridLevel.CellSize.x / 3, (this.gridLevel.Level - 1) * levelZShift, center.z));
                        }
                    }
                }
            }
        }
    }

    public class GridCell
    {
        public int level;
        public Vector2 center;

        public GridCell parentCell;
        public List<GridCell> childCells = new List<GridCell>();
        public GridCell[] neighbors = new GridCell[4]; // 0 = above, 1 = left, 2 = below, 3 = right

        public Dictionary<string, AbstractGridData> data = new Dictionary<string, AbstractGridData>();
        
        public GridCell(GridLevel level, Vector2 center) {
            this.center = center;
            this.level = level.Level;

            this.setupLevelData(level);
        }

        private void setupLevelData(GridLevel level){
            foreach(KeyValuePair<string, DataType> dataEntry in level.data) {
                switch(dataEntry.Value) {
                    case Float:
                        this.dataFloat.Add(dataEntry.Key, new GridDaa<Float>(0.0));
                        break;
                    case Int:
                        this.dataInt.Add(dataEntry.Key, 0);
                        break;
                    case Bool:
                        this.dataBool.Add(dataEntry.Key, false);
                        break;
                    case Object:
                        this.dataObject.Add(dataEntry.Key, null);
                        break;
                    case Object:
                        this.dataObject.Add(dataEntry.Key, null);
                        break;
                }
            }
        }

        public void AddData(string name, object data)
        {
            this.data.Add(name, data);
        }

        public object GetData(string name)
        {
            return this.data[name];
        }
    }

    public abstract class AbstractGridData {
        public string name;

        public AbstractGridData(string name) {
            this.name = name;
        }
    }

    public class GridData<T> : AbstractGridData {
        public T data;

        public GridData(string name, T data) : base(name) {
            this.data = data;
        }
    }
    
    public enum DataType {
        Float,
        Int,
        Bool,
        Vec2,
        Object
    }
}