using UnityEngine;

public class Grid
{
    public GridLevel Level;

    public List<GridCell> Cells = new List<GridCell>();

    public Grid(GridLevel level)
    {
        this.Level = level;

        for (int x = 0; x < Level.Dimensions.x; x++)
        {
            for (int y = 0; y < Level.Dimensions.y; y++)
            {
                Cells.Add(new GridCell(x, y, this));
            }
        }
    }

    public void Add(GridObject gridObject)
    {
        if (Level.IsAllowedType(gridObject))
        {
            ContainedObjects.Add(gridObject);
        }
    }

    public void Remove(GridObject gridObject)
    {
        if (Level.IsAllowedType(gridObject))
        {
            ContainedObjects.Remove(gridObject);
        }
    }

}