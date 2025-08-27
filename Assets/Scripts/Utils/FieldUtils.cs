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
    private static readonly List<CellType> VillageCells = new List<CellType>() {
        CellType.Village,
        CellType.VillageLevel2,
    };
    
    private static readonly List<CellType> SawmillCells = new List<CellType>() {
        CellType.Sawmill,
    };

    private static readonly List<CellType> CantBecomeRowCells = new List<CellType>() {
        CellType.Empty,
        CellType.Ice,
        CellType.Box,
        CellType.Crystal,
        CellType.Slime,
        CellType.LockedMetaCell,
        CellType.LockedCoreCell,
    };
    
    private static readonly List<CellType> ResourcesCells = new List<CellType>() {
        CellType.Wood,
        CellType.Stone,
        CellType.Wheat,
        CellType.Metal
    };
    
    private static readonly List<CellType> CantDestroyInRowCells = new List<CellType>() {
        CellType.LockedCoreCell,
        CellType.LockedMetaCell,
        CellType.GoldMine,
        CellType.CrystalMine,
        CellType.Empty,
    };
    private static readonly List<CellType> CanBeHammeredOrExploded = new List<CellType>() {
        CellType.Wood,
        CellType.Stone,
        CellType.Wheat,
        CellType.Metal,
        
        CellType.Box,
        CellType.Ice,
        CellType.Slime,
        CellType.Slime,
        
        
        CellType.Forest,
        CellType.ForestLevel2,
        CellType.Mountain,
        CellType.MountainLevel2,
        CellType.FieldOfWheat,
        CellType.FieldOfWheatLevel2,
        CellType.MetalMines,
        CellType.MetalMinesLevel2,
        
        CellType.Sawmill,
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

    public static List<Vector2Int> GetAllEmptyCells(CellType[,] field) {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int i = 0; i < field.GetLength(0); i++) {
            for (int j = 0; j < field.GetLength(1); j++) {
                if (CanPlaceOnCell(field[i, j]))
                    emptyCells.Add(new Vector2Int(i, j));
            }
        }

        return emptyCells;
    }

    public static List<Vector2Int> GetRandomEmptyCellsWithoutSomeCells(CellType[,] field, int amount, List<Vector2Int> pieceCells) {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                if(CanPlaceOnCell(field[i,j])) 
                    emptyCells.Add(new Vector2Int(i, j));
            }
        }

        if (amount == 0)
            return emptyCells;
        foreach (Vector2Int cell in pieceCells) {
            emptyCells.Remove(cell);
        }
       
        
        emptyCells = emptyCells.OrderBy(_ => Random.Range(0, 1f)).ToList();
        return emptyCells.Take(Mathf.Min(amount, emptyCells.Count)).ToList();
    }

    public static List<Vector2Int> GetCellsFromUltRows(int maxStars) {
        var pieceData = GameFieldManager.Instance.GetRandomCurrentPieceData();
        int fieldLengthOffset = pieceData.Cells.GetLength(0) - 1;
        int fieldHeightOffset = pieceData.Cells.GetLength(1) - 1;

        var field = GameFieldManager.Instance._field;
        List<Vector2Int> placedCells = new List<Vector2Int>();
        int currentColumn = 0;
        int currentRow = 0;
        Vector2Int ignoredCell = -Vector2Int.one;
        if (pieceData.FormName == "ZRotated" || pieceData.FormName == "Z" || pieceData.FormName == "smallSquare"
            || pieceData.FormName == "S"|| pieceData.FormName == "SRotated") {
            Vector2Int[] verticalDiractions = { new(-1, -1), new(-1, 1), new(1, -1), new(1, 1) };

            for (int i = fieldLengthOffset; i < field.GetLength(0) - fieldLengthOffset; i++) {
                if (currentRow != 0)
                    break;
                for (int j = fieldHeightOffset; j < field.GetLength(1) - fieldHeightOffset; j++) {
                    if (currentRow != 0)
                        break;
                    foreach (Vector2Int verticalOffset in verticalDiractions) {
                        if (CanPlaceOnCell(field[i + verticalOffset.x, j + verticalOffset.y])) {
                            ignoredCell = new Vector2Int(i + verticalOffset.x, j + verticalOffset.y);
                            currentColumn = j;
                            currentRow = i;
                            break;
                        }
                    }
                }
            }
        } else {
            currentColumn = Random.Range(fieldLengthOffset, field.GetLength(0) - fieldLengthOffset);
            currentRow = Random.Range(fieldHeightOffset, field.GetLength(1) - fieldHeightOffset);
        }
        

        for (int i = 0; i < field.GetLength(0); i++) {
            if (CanPlaceOnCell(field[currentColumn, i]))
                placedCells.Add(new Vector2Int(currentColumn, i));
        }

        for (int i = 0; i < field.GetLength(1); i++) {
            if (CanPlaceOnCell(field[i, currentRow]) || i == currentColumn)
                placedCells.Add(new Vector2Int(i, currentRow));
        }

        if (maxStars - placedCells.Count > 0) {
            var randomEmptyCells = new List<Vector2Int>();
            if (ignoredCell == -Vector2Int.one)
                randomEmptyCells = GetRandomEmptyCellsWithoutSomeCells(field, maxStars - placedCells.Count, placedCells);
            else {
                var ignoredCellsArray = placedCells;
                ignoredCellsArray.Add(ignoredCell);
                randomEmptyCells = GetRandomEmptyCellsWithoutSomeCells(field, maxStars - placedCells.Count, ignoredCellsArray);
            }

            foreach (var cell in randomEmptyCells) {
                placedCells.Add(cell);
            }
        }

        
        return placedCells;
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
    
    public static List<Vector2Int> PlacedPieceCells(CellType[,] field, PieceData data, Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0)
            return null;

        if (pos.x + data.Cells.GetLength(0) - 1 >= field.GetLength(0))
            return null;

        if (pos.y + data.Cells.GetLength(1) - 1 >= field.GetLength(1))
            return null;

        List<Vector2Int> placedCells = new List<Vector2Int>();
        for (int x = 0; x < data.Cells.GetLength(0); x++) {
            for (int y = 0; y < data.Cells.GetLength(1); y++) {
                if (data.Cells[x, y] && CanPlaceOnCell(field[pos.x + x, pos.y + y]))
                    placedCells.Add(new Vector2Int(pos.x + x, pos.y + y));
            }
        }

        return placedCells;
    }
    
    public static List<Vector2Int> PlacedPieceCellsWithoutResource(CellType[,] field, PieceData data, Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0)
            return null;

        if (pos.x + data.Cells.GetLength(0) - 1 >= field.GetLength(0))
            return null;

        if (pos.y + data.Cells.GetLength(1) - 1 >= field.GetLength(1))
            return null;

        List<Vector2Int> placedCells = new List<Vector2Int>();
        for (int x = 0; x < data.Cells.GetLength(0); x++) {
            for (int y = 0; y < data.Cells.GetLength(1); y++) {
                if (data.Cells[x, y] && CanPlaceOnCell(field[pos.x + x, pos.y + y]) && CanPlaceOnCell(field[pos.x + x, pos.y + y]))
                    placedCells.Add(new Vector2Int(pos.x + x, pos.y + y));
            }
        }
//calculate need row with unbreckeable blocks and boxes
        return placedCells;
    }
    public static bool CanPlaceOnCell(CellType cellType) => CanPlaceOnCells.Contains(cellType);
    public static bool CantBecomeRow(CellType cellType) => CantBecomeRowCells.Contains(cellType);
    public static bool CantDestroyInRow(CellType cellType) => CantDestroyInRowCells.Contains(cellType);
    public static bool CanHammerOrExplode(CellType cellType) => CanBeHammeredOrExploded.Contains(cellType);
    public static bool IsResourceCell(CellType cellType) => ResourcesCells.Contains(cellType);
    
    public static bool IsVillageCell(CellType cellType) => VillageCells.Contains(cellType);
    public static bool IsSawmillCell(CellType cellType) => SawmillCells.Contains(cellType);
    public static Vector2Int ClampToCoord(Vector3 coord) => new(Mathf.RoundToInt(coord.x) / CellSize, Mathf.RoundToInt(coord.z) / CellSize);
}