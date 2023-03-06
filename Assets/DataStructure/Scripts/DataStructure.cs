using UnityEngine;

namespace DataStructure
{
    public class DataStructure : MonoBehaviour
    {

    }

    public class Grid
    {
        public int level;
        public Vector2 size;
        public int cellsPerAxis;

        public Grid(int level, Vector2 size, int cellsPerAxis)
        {
            this.level = level;
            this.size = size;
            this.cellsPerAxis = cellsPerAxis;

            createCells();
        }

        private void createCells()
        {
            float stepX = size.x / cellsPerAxis;
            float stepY = size.y / cellsPerAxis;
            
            for (int y = 0; y < cellsPerAxis; y++)
            {
                for (int x = 0; x < cellsPerAxis; x++)
                {
                    // Calculate position using size
                    Vector2 pos = new Vector2(x * stepX, y * stepY);
                
                }
            }
        }


    }

    public struct GridCell
    {
        public int level;
        public Vector2 position;
        public Vector2 size;

        public List<GridData> data;

        public GridCell(int level, Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
            this.level = level;
            this.data = new List<GridData>();
        }

        public setData(GridData data)
        {
            this.data.Add(data);
        }
    }

    public abstract struct GridData<T>
    {
        public string name;
        public T data;

        public GridData(string name, T data)
        {
            this.name = name;
            this.data = data;
        }
    }


}