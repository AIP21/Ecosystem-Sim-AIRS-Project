using UnityEngine;

[CreateAssetMenu(menuName = "Ecosystem Sim AIRS Project/Grid Level")]
public class GridLevel : ScriptableObject
{
    public Vector2 Dimensions;
    public int Level;

    public List<Type> AllowedTypes = new List<Type>();

    public GridLevel(int level, Vector2 dimensions)
    {
        this.Level = level;
        this.Dimensions = dimensions;
    }

    public bool IsAllowedType(Type type)
    {
        return AllowedTypes.Contains(type);
    }

    public bool IsAllowedType(GridObject gridObject)
    {
        return IsAllowedType(gridObject.GetType());
    }


}