using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class FieldUtils {
    public const int CellSize = 1;
    public static readonly Vector2Int[] Directions = {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    private static readonly List<CellType> CanPlaceOnCells = new List<CellType>() {
        CellType.Empty,
        CellType.Crystal,
        CellType.Ice,
        CellType.Slime
    };

    private static readonly List<CellType> CantBecomeRowCells = new List<CellType>() {
        CellType.Empty,
        CellType.Ice,
        CellType.Box,
        CellType.Crystal,
        CellType.Slime,
        CellType.LockedMetaCell
    };
    
    private static readonly List<CellType> ResourcesCells = new List<CellType>() {
        CellType.Wood,
        CellType.Stone,
        CellType.Wheat,
        CellType.Metal
    };
    
    private static readonly List<CellType> CantDestroyInRowCells = new List<CellType>() {
        CellType.GoldMine,
        CellType.CrystalMine,
        CellType.Empty,
    };

    public static Vector2Int GetRandomEmptyCell(CellType[,] field)
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                if(CanPlaceOnCell(field[i,j]))
                    emptyCells.Add(new Vector2Int(i,j));
            }
        }
        return emptyCells[Random.Range(0, emptyCells.Count)];
    }
    
    public static List<Vector2Int> GetRandomEmptyCells(CellType[,] field, int amount)
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                if(CanPlaceOnCell(field[i,j])) {
                    emptyCells.Add(new Vector2Int(i, j));
                }
            }
        }

        if (amount == 0)
            return emptyCells;
        emptyCells = emptyCells.OrderBy(_ => Random.Range(0, 1f)).ToList();
        return emptyCells.Take(Mathf.Min(amount, emptyCells.Count)).ToList();
    }
    
    public static IEnumerable<Vector2Int> GetCellsAround(CellType[,] field, Vector2Int coord) {
        return Directions.Select(pos => coord + pos).Where(combined => IsInsideField(field, combined));
    }

    public static bool IsInsideField(CellType[,] field, Vector2Int coord) {
        return coord.y < field.GetLength(0) && coord.x < field.GetLength(1) && coord.x >= 0 && coord.y >= 0;
    }
    public static bool CanPlacePiece(CellType[,] field, PieceData data, Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0)
            return false;

        if (pos.x + data.Cells.GetLength(0) - 1 >= field.GetLength(0))
            return false;

        if (pos.y + data.Cells.GetLength(1) - 1 >= field.GetLength(1))
            return false;

        for (int x = 0; x < data.Cells.GetLength(0); x++) {
            for (int y = 0; y < data.Cells.GetLength(1); y++) {
                if (data.Cells[x, y] && !CanPlaceOnCell(field[pos.x + x, pos.y + y]))
                    return false;
            }
        }

        return true;
    }
    public static bool CanPlaceOnCell(CellType cellType) => CanPlaceOnCells.Contains(cellType);
    public static bool CantBecomeRow(CellType cellType) => CantBecomeRowCells.Contains(cellType);
    public static bool CantDestroyInRow(CellType cellType) => CantDestroyInRowCells.Contains(cellType);
    public static bool IsResourceCell(CellType cellType) => ResourcesCells.Contains(cellType);
    
    public static Vector2Int ClampToCoord(Vector3 coord) => new(Mathf.RoundToInt(coord.x) / CellSize, Mathf.RoundToInt(coord.z) / CellSize);
}