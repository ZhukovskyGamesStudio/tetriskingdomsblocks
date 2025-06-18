using System;
using System.Collections.Generic;
using System.Linq;

public class SameCellsGroupCalculater
{
   private static readonly (int row, int col)[] directions = 
    {
        (-1, 0), // вверх
        (1, 0),  // вниз
        (0, -1), // влево
        (0, 1)   // вправо
    };
    public static (int[,], List<List<(int row, int col)>>) FindConnectedCellTypeGroups(CellType[,] grid)
    {
        var result = new List<List<(int, int)>>();
        var cellsIndex = new int[grid.GetLength(0), grid.GetLength(1)];
        if (grid == null || grid.Length == 0) return (cellsIndex,result);

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        bool[,] visited = new bool[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (!visited[i, j] && grid[i, j] != CellType.Empty) 
                {
                    CellType cellType = grid[i, j];
                    var resourceType = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                        (c => c.CellType == cellType).ResourcesForDestroy[0].ResourceType;//make afk collect info in config
                    var group = BFSWithCoordinates(grid, visited, i, j, resourceType);

                    result.Add(group);
                    foreach (var (row,col) in group)
                    cellsIndex[row, col] = result.Count;
                }
            }
        }

        return (cellsIndex,result);
    }

    private static List<(int row, int col)> BFSWithCoordinates(CellType[,] grid, bool[,] visited, int startRow, int startCol, ResourceType targetType)
    {
        var queue = new Queue<(int, int)>();
        queue.Enqueue((startRow, startCol));
        visited[startRow, startCol] = true;
        var group = new List<(int, int)> { (startRow, startCol) };

        while (queue.Count > 0)
        {
            var (row, col) = queue.Dequeue();

            foreach (var dir in directions)
            {
                int newRow = row + dir.row;
                int newCol = col + dir.col;
               
                if (newRow >= 0 && newRow < grid.GetLength(0) && 
                    newCol >= 0 && newCol < grid.GetLength(1) && 
                    !visited[newRow, newCol] && grid[newRow, newCol] != CellType.Empty)
                {
                    var resourceType =MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                        (c => c.CellType == grid[newRow, newCol]).ResourcesForDestroy;//make afk collect info in config
                    if(resourceType.Length == 0 || resourceType[0].ResourceType != targetType)continue;
                    visited[newRow, newCol] = true;
                    queue.Enqueue((newRow, newCol));
                    group.Add((newRow, newCol));
                }
            }
        }

        return group;
    }
}
