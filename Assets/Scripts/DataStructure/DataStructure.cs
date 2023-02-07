using UnityEngine;

public class DataStructure : MonoBehaviour
{
    #region Public Variables
    public List<Vector2> GridLevels = new List<Vector2>();

    public List<GridLevel> Grids = new List<GridLevel>();
    #endregion

    public void Start()
    {
        // Create the grids
        foreach (Vector2 gridLevel in GridLevels)
        {
            Grids.Add(new Grid(gridLevel));
        }
    }

    public void FixedUpdate()
    {

    }

    public void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Click"))
        {
            Debug.Log("You clicked the button!");
        }
    }
}