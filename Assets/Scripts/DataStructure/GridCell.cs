
public class GridCell
{
    private Vector2 center;
    public Vector2 Center
    {
        get { return center; }
        set { center = value; }
    }

    private Vector2 dimensions;
    public Vector2 Dimensions
    {
        get { return dimensions; }
        set { dimensions = value; }
    }

    private Grid grid;
    public Grid Grid
    {
        get { return grid; }
        set { grid = value; }
    }

    public List<GridObject> Objects = new List<GridObject>();

    public GridCell(Vector2 center, Vector2 dimensions, Grid grid)
    {
        this.Center = center;
        this.Dimensions = dimensions;
        this.Grid = grid;
    }

    public void Add(GridObject gridObject)
    {
        Cells.Add(gridObject);
    }

    
}