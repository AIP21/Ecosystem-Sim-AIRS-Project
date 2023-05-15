using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bounds2D
{
     public Vector2 center;
    public Vector2 extents;

    public Bounds2D(Vector2 center, Vector2 extents)
    {
        this.center = center;
        this.extents = extents;
    }

    public Vector2 min { get { return center - extents; } }
    public Vector2 max { get { return center + extents; } }

    public bool Contains(Vector2 point)
    {
        return Mathf.Abs(point.x - center.x) <= extents.x && Mathf.Abs(point.y - center.y) <= extents.y;
    }

    public bool Intersects(Bounds2D other)
    {
        return Mathf.Abs(other.center.x - center.x) < extents.x + other.extents.x &&
               Mathf.Abs(other.center.y - center.y) < extents.y + other.extents.y;
    }

    public Vector2 RandomPointInside()
    {
        return center + new Vector2(Random.Range(-extents.x, extents.x), Random.Range(-extents.y, extents.y));
    }

    public float GetArea()
    {
        return 4 * extents.x * extents.y;
    }

    public bool Contains(Bounds2D other)
    {
        return Contains(other.min) && Contains(other.max);
    }

    public Bounds ToBounds()
    {
        return new Bounds(center, new Vector3(extents.x * 2f, extents.y * 2f, 0f));
    }

    public Bounds2D Expand(float amount)
    {
        return new Bounds2D(center, extents + new Vector2(amount, amount));
    }
}