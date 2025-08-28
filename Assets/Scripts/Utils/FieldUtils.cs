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
       CellType.Empty
    };
    private static readonly List<CellType> DontCountInUltimateRowCells = new List<CellType>() {
        CellType.LockedCoreCell,
        CellType.LockedMetaCell,
        CellType.GoldMine,
        CellType.CrystalMine,
        CellType.Box,
        CellType.Ice,
        CellType.Slime,
        CellType.Crystal,
        CellType.Empty
    };
    private static readonly List<CellType> CanBeHammeredOrExploded = new List<CellType>() {
        CellType.Wood,
        CellType.Stone,
        CellType.Wheat,
        CellType.Metal,
        
        CellType.Box,
        CellType.Ice,
        CellType.Slime,
        CellType.Crystal,
        
        
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
        if (amount <= 0) return null;
        for (int i = 0; i < field.GetLength(0); i++) {
            for (int j = 0; j < field.GetLength(1); j++) {
                if (CanPlaceOnCell(field[i, j]))
                    emptyCells.Add(new Vector2Int(i, j));
            }
        }

        foreach (Vector2Int cell in pieceCells) {
            emptyCells.Remove(cell);
        }

        emptyCells = emptyCells.OrderBy(_ => Random.Range(0, 1f)).ToList();
        return emptyCells.Take(Mathf.Min(amount, emptyCells.Count)).ToList();
    }

    public static List<Vector2Int> GetCellsFromUltRows(int maxStars) {
        var pieceData = GameFieldManager.Instance.GetRandomCurrentPieceData();
//Debug.Log("new ult ..............................");
        
        var field = GameFieldManager.Instance._field;
        List<Vector2Int> placedCells = new List<Vector2Int>();
        List<int> currentColumnToDestroy = new List<int>();
        List<int> currentRowToDestroy = new List<int>();
        Vector2Int ignoredCell = -Vector2Int.one;

        Dictionary<int, int> xPositions = new Dictionary<int, int>();
        Dictionary<int, int> yPositions = new Dictionary<int, int>();
        
        var placedPiecePositions = PieceUtils.CanPlacedPieceWhenDestroyCells(field, pieceData);
        foreach (var placedCellPosition in placedPiecePositions) {
           if(!xPositions.TryAdd(placedCellPosition.x,1))
               xPositions[placedCellPosition.x]++;
           
           if(!yPositions.TryAdd(placedCellPosition.y,1))
               yPositions[placedCellPosition.y]++;
        }

        foreach (var xPos in xPositions) {
            if (xPos.Value > 1) {
                 currentColumnToDestroy.Add(xPos.Key);
              //   Debug.Log("key"+xPos.Key+"value"+xPos.Value);
            }
               
             
        }
        foreach (var yPos in yPositions) {
            if (yPos.Value > 1) {
                currentRowToDestroy.Add(yPos.Key);
            //    Debug.Log("key" + yPos.Key + "value" + yPos.Value);
            }
        }

        List<Vector2Int> placedCellsInEnd = new List<Vector2Int>();

        bool xIsMainDirection = currentColumnToDestroy.Count == pieceData.Cells.GetLength(0);
        bool yIsMainDirection = currentRowToDestroy.Count == pieceData.Cells.GetLength(1);

        if ((!yIsMainDirection && !xIsMainDirection) || xIsMainDirection) {
            foreach (var currentColumn in currentColumnToDestroy) {
                
                bool allCellIsCanPlaced = true;
                for (int i = 0; i < field.GetLength(0); i++) {
                    if (!CanPlaceOnCell(field[currentColumn, i])) {
                        allCellIsCanPlaced = false;
                        break;
                    }
                }

                if (allCellIsCanPlaced)
                    continue;
                
                for (int i = 0; i < field.GetLength(0); i++) {
                    if (CanPlaceOnCell(field[currentColumn, i])) {
                        placedCells.Add(new Vector2Int(currentColumn, i));
                        bool isAddThisCellInEnd = true;
                        for (int j = 0; j < field.GetLength(1); j++) {
                            if (DontCountInUltimateRow(field[j, i]) && j != currentColumn) {
                                isAddThisCellInEnd = false;
                                break;
                            }
                        }

                        if (isAddThisCellInEnd)
                            placedCellsInEnd.Add(new Vector2Int(currentColumn, i));
                    } else if (field[currentColumn, i] == CellType.Box)
                        placedCellsInEnd.Add(new Vector2Int(currentColumn, i));
                }

                foreach (var endCell in placedCellsInEnd) {
                    placedCells.Add(endCell);
                }

                placedCellsInEnd = new List<Vector2Int>();
            }
        }
      //  Debug.Log("y main"+yIsMainDirection + " x main "+xIsMainDirection);
        if ((!yIsMainDirection && !xIsMainDirection) || (yIsMainDirection && !xIsMainDirection)) {
            foreach (var currentRow in currentRowToDestroy) {
                bool allCellIsCanPlaced = true;
                bool isAllRowIsFull = true;
                for (int i = 0; i < field.GetLength(1); i++) {
                    if (!CanPlaceOnCell(field[i, currentRow])) {
                        allCellIsCanPlaced = false;
                        if(!isAllRowIsFull)
                            break;
                    }
                    else if (!currentColumnToDestroy.Contains(i)) {
                        isAllRowIsFull = false;
                        if(!allCellIsCanPlaced)
                            break;
                    }
                }

                if (allCellIsCanPlaced || isAllRowIsFull)
                    continue;
                
                for (int i = 0; i < field.GetLength(1); i++) {
                  //  Debug.Log(currentColumnToDestroy.Contains(i) + "   " + i + "," + currentRow);
                    if (CanPlaceOnCell(field[i, currentRow]) || (currentColumnToDestroy.Count != 0 && currentColumnToDestroy.Contains(i))) {
                        placedCells.Add(new Vector2Int(i, currentRow));

                        bool isAddThisCellInEnd = true;
                        for (int j = 0; j < field.GetLength(0); j++) {
                            if (DontCountInUltimateRow(field[i, j]) && j != currentRow) {
                                isAddThisCellInEnd = false;
                                break;
                            }
                        }

                        if (isAddThisCellInEnd)
                            placedCellsInEnd.Add(new Vector2Int(i, currentRow));
                    } else if (field[i, currentRow] == CellType.Box)
                        placedCellsInEnd.Add(new Vector2Int(i, currentRow));
                }

                foreach (var endCell in placedCellsInEnd) {
                    placedCells.Add(endCell);
                }

                placedCellsInEnd = new List<Vector2Int>();
            }
        }
         

        if (maxStars - placedCells.Count > 0) {
            var randomEmptyCells = new List<Vector2Int>();
                randomEmptyCells = GetRandomEmptyCellsWithoutSomeCells(field, maxStars - placedCells.Count, placedPiecePositions);

            if (randomEmptyCells != null)
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
    
    public static bool CanDestroyCellsAndPlacePiece(CellType[,] field, PieceData data, Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0)
            return false;

        if (pos.x + data.Cells.GetLength(0) - 1 >= field.GetLength(0))
            return false;

        if (pos.y + data.Cells.GetLength(1) - 1 >= field.GetLength(1))
            return false;

        for (int x = 0; x < data.Cells.GetLength(0); x++) {
            for (int y = 0; y < data.Cells.GetLength(1); y++) {
                if (data.Cells[x, y] && !CanPlaceOnCell(field[pos.x + x, pos.y + y]) && !IsResourceCell(field[pos.x + x, pos.y + y]))
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
                if (data.Cells[x, y] && (CanPlaceOnCell(field[pos.x + x, pos.y + y]) || IsResourceCell(field[pos.x + x, pos.y + y])))
                    placedCells.Add(new Vector2Int(pos.x + x, pos.y + y));
            }
        }
//calculate need row with unbreckeable blocks and boxes
        return placedCells;
    }
    public static bool CanPlaceOnCell(CellType cellType) => CanPlaceOnCells.Contains(cellType);
    public static bool CantBecomeRow(CellType cellType) => CantBecomeRowCells.Contains(cellType);
    public static bool CantDestroyInRow(CellType cellType) => CantDestroyInRowCells.Contains(cellType);
    public static bool DontCountInUltimateRow(CellType cellType) => DontCountInUltimateRowCells.Contains(cellType);
    public static bool CanHammerOrExplode(CellType cellType) => CanBeHammeredOrExploded.Contains(cellType);
    public static bool IsResourceCell(CellType cellType) => ResourcesCells.Contains(cellType);
    
    public static bool IsVillageCell(CellType cellType) => VillageCells.Contains(cellType);
    public static bool IsSawmillCell(CellType cellType) => SawmillCells.Contains(cellType);
    public static Vector2Int ClampToCoord(Vector3 coord) => new(Mathf.RoundToInt(coord.x) / CellSize, Mathf.RoundToInt(coord.z) / CellSize);
}