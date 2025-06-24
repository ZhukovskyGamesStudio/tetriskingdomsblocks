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
        CellType.Ice
    };

    private static readonly List<CellType> CantBecomeRowCells = new List<CellType>() {
        CellType.Empty,
        CellType.Ice,
        CellType.Box
    };

    public static IEnumerable<Vector2Int> GetCellsAround(CellType[,] field, Vector2Int coord) {
        return Directions.Select(pos => coord + pos).Where(combined => IsInsideField(field, combined));
    }

    public static bool IsInsideField(CellType[,] field, Vector2Int coord) {
        return coord.y < field.GetLength(0) && coord.x < field.GetLength(1) && coord.x > 0 && coord.y >= 0;
    }

    public static bool CanPlaceOnCell(CellType cellType) => CanPlaceOnCells.Contains(cellType);
    public static bool CantBecomeRow(CellType cellType) => CantBecomeRowCells.Contains(cellType);
    
    public static Vector2Int ClampToCoord(Vector3 coord) => new(Mathf.RoundToInt(coord.x) / CellSize, Mathf.RoundToInt(coord.z) / CellSize);
}