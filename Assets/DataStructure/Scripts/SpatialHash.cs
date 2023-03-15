// Adapted from https://github.com/DoctorB/spatial-hash
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

// namespace DataStructure
// {
//     public class SpatialHash
//     {
//         /* the square cell gridLength of the grid. Must be larger than the largest shape in the space. */
//         private float gridHeightRes;
//         private float gridWidthRes;
//         private decimal invCellSize;

//         public SpatialHash(int _width, int _height, int _cellSize)
//         {
//             gridWidthRes = _width;
//             gridHeightRes = _height;

//             CellSize = _cellSize;
//             invCellSize = (decimal)1 / CellSize;

//             GridWidth = (int)Math.Ceiling(_width * invCellSize);
//             GridHeight = (int)Math.Ceiling(_height * invCellSize);

//             GridLength = GridWidth * GridHeight;

//             Grid = new List<List<GridObject>>(GridLength);

//             for (int i = 0; i < GridLength; i++)
//                 Grid.Add(new List<GridObject>());

//         }

//         public void Add(GridObject obj)
//         {
//             addAt(obj, GetIndex1DVec(clampToGrid(obj.pos.x, obj.y)));
//         }

//         public void Remove(GridObject obj)
//         {
//             removeIndices(obj);
//         }

//         public void UpdateObject(GridObject obj)
//         {
//             updateIndices(obj, AABBToGrid(obj.aabb.min, obj.aabb.max));
//         }

//         public List<GridObject> GetAllSharingCells(GridObject obj)
//         {
//             var collidingBodies = new List<GridObject>();
//             foreach (int i in obj.gridIndex)
//             {
//                 if (Grid[i].Count == 0)
//                     continue;

//                 foreach (var cbd in Grid[i].ToArray())
//                 {
//                     if (cbd == obj)
//                         continue;
//                     collidingBodies.Add(cbd);
//                 }
//             }
//             return collidingBodies;
//         }

//         public bool IsSharingCell(GridObject obj)
//         {
//             foreach (int i in obj.gridIndex)
//             {
//                 if (Grid[i].Count == 0)
//                     continue;

//                 foreach (var cbd in Grid[i].ToArray())
//                 {
//                     if (cbd == obj)
//                         continue;
//                     return true;
//                 }
//             }
//             return false;
//         }

//         public int GetIndex1DVec(Vector2 pos)
//         {
//             return (int)(Math.Floor(pos.x * (float)invCellSize) + GridWidth * Math.Floor(pos.y * (float)invCellSize));
//         }

//         private int getIndex(float pos)
//         {
//             return (int)(pos * (float)invCellSize);
//         }

//         private int getIndex1D(int x, int y)
//         {
//             // i = x + w * y;  x = i % w; y = i / w;
//             return (int)(x + GridWidth * y);
//         }

//         private void updateIndices(GridObject b, List<int> array)
//         {
//             foreach (int i in b.gridIndex)
//             {
//                 removeAt(b, i);
//             }
//             //b.gridIndex.splice( 0, b.gridIndex.length );
//             b.gridIndex.Clear();

//             foreach (int index in array)
//             {
//                 addAt(b, index);
//             }
//         }

//         private void addAt(GridObject b, int cellPos)
//         {
//             Grid[cellPos].Add(b);
//             b.gridIndex.Add(cellPos);
//         }

//         private void removeIndices(GridObject b)
//         {
//             foreach (int i in b.gridIndex)
//             {
//                 removeAt(b, i);
//             }
//             //b.gridIndex.splice( 0, b.gridIndex.length );
//             b.gridIndex.Clear();
//         }

//         private void removeAt(GridObject b, int pos)
//         {
//             Grid[pos].Remove(b);
//         }

//         private bool posInGrid(int num)
//         {
//             return !(num < 0 || num >= GridLength);
//         }

//         public Vector2 clampToGrid(float x, float y)
//         {
//             Vector2 _vec = new Vector2(x, y);
//             _vec.x = MathHelper.Clamp(_vec.x, 0, gridWidthRes - 1);
//             _vec.y = MathHelper.Clamp(_vec.y, 0, gridHeightRes - 1);
//             return _vec;
//         }

//         private List<int> AABBToGrid(Vector2 min, Vector2 max)
//         {
//             var arr = new List<int>();
//             int aabbMinX = MathHelper.Clamp(getIndex(min.x), 0, GridWidth - 1);
//             int aabbMinY = MathHelper.Clamp(getIndex(min.y), 0, GridHeight - 1);
//             int aabbMaxX = MathHelper.Clamp(getIndex(max.x), 0, GridWidth - 1);
//             int aabbMaxY = MathHelper.Clamp(getIndex(max.y), 0, GridHeight - 1);

//             int aabbMin = getIndex1D(aabbMinX, aabbMinY);
//             int aabbMax = getIndex1D(aabbMaxX, aabbMaxY);

//             arr.Add(aabbMin);
//             if (aabbMin != aabbMax)
//             {
//                 arr.Add(aabbMax);
//                 int lenX = aabbMaxX - aabbMinX + 1;
//                 int lenY = aabbMaxY - aabbMinY + 1;
//                 for (int x = 0; x < lenX; x++)
//                 {
//                     for (int y = 0; y < lenY; y++)
//                     {
//                         if ((x == 0 && y == 0) || (x == lenX - 1 && y == lenY - 1))
//                             continue;
//                         arr.Add(getIndex1D(x, y) + aabbMin);
//                     }
//                 }
//             }
//             return arr;
//         }

//         /* DDA line algorithm. @author playchilla.com */
//         public List<int> LineToGrid(float x1, float y1, float x2, float y2)
//         {
//             var arr = new List<int>();

//             int gridPosX = getIndex(x1);
//             int gridPosY = getIndex(y1);

//             if (!posInGrid(gridPosX) || !posInGrid(gridPosY))
//                 return arr;

//             arr.Add(getIndex1D(gridPosX, gridPosY));

//             float dirX = x2 - x1;
//             float dirY = y2 - y1;
//             float distSqr = dirX * dirX + dirY * dirY;
//             if (distSqr < float.Epsilon) // WAS: 0.00000001
//                 return arr;

//             float nf = (float)(1 / Math.Sqrt(distSqr));
//             dirX *= nf;
//             dirY *= nf;

//             float deltaX = CellSize / Math.Abs(dirX);
//             float deltaY = CellSize / Math.Abs(dirY);

//             float maxX = gridPosX * CellSize - x1;
//             float maxY = gridPosY * CellSize - y1;
//             if (dirX >= 0)
//                 maxX += CellSize;
//             if (dirY >= 0)
//                 maxY += CellSize;
//             maxX /= dirX;
//             maxY /= dirY;

//             int stepX = Math.Sign(dirX);
//             int stepY = Math.Sign(dirY);

//             int gridGoalX = getIndex(x2);
//             int gridGoalY = getIndex(y2);
//             int currentDirX = gridGoalX - gridPosX;
//             int currentDirY = gridGoalY - gridPosY;

//             while (currentDirX * stepX > 0 || currentDirY * stepY > 0)
//             {
//                 if (maxX < maxY)
//                 {
//                     maxX += deltaX;
//                     gridPosX += stepX;
//                     currentDirX = gridGoalX - gridPosX;
//                 }
//                 else
//                 {
//                     maxY += deltaY;
//                     gridPosY += stepY;
//                     currentDirY = gridGoalY - gridPosY;
//                 }

//                 if (!posInGrid(gridPosX) || !posInGrid(gridPosY))
//                     break;

//                 arr.Add(getIndex1D(gridPosX, gridPosY));
//             }
//             return arr;
//         }

//         public void Clear()
//         {
//             foreach (var cell in Grid)
//             {
//                 if (cell.Count > 0)
//                 {
//                     foreach (var co in cell)
//                     {
//                         co.gridIndex.Clear();
//                     }
//                     cell.Clear();
//                 }
//             }
//         }

//         public int CellSize { get; set; }

//         /* the world space width */
//         public int GridWidth { get; set; }

//         /* the world space height */
//         public int GridHeight { get; set; }

//         /* the number of buckets (i.e. cells) in the spatial grid */
//         public int GridLength { get; set; }
//         /* the array-list holding the spatial grid buckets */
//         public List<List<GridObject>> Grid { get; set; }
//     }
// }