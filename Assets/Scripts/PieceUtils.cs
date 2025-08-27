using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class PieceUtils {
    public static PieceData GetNewMetaPiece(CellTypeInfo guaranteed) {
        var cellsToSpawn = MetaFieldManager.Instance != null? MetaFieldManager.Instance._currentCellsToSpawnInMeta : GameFieldManager.Instance._currentCellsToSpawnInMeta;
        var chancesToSpawn = MetaFieldManager.Instance != null?MetaFieldManager.Instance.CellsChanceToSpawnInMeta:GameFieldManager.Instance.CellsChanceToSpawnInMeta;

        return GetNewPiece(cellsToSpawn, chancesToSpawn, guaranteed);
    }

    public static PieceData GetNewCorePiece(CellTypeInfo guaranteed) {
        var cellsToSpawn = GameFieldManager.Instance._currentCellsToSpawn;
        var chancesToSpawn = GameFieldManager.Instance.CellsChanceToSpawn;

        return GetNewPiece(cellsToSpawn, chancesToSpawn, guaranteed);
    }

    public static PieceData GetNewCorePiece(List<CellTypeInfo> guaranteedPieces) {
        if (guaranteedPieces.Count > 0) {
            var next = guaranteedPieces[0];
            guaranteedPieces.RemoveAt(0);
            return GetNewCorePiece(next);
        }

        return GetNewCorePiece(guaranteed: null);
    }
    
    
    public static PieceData GetExactPiece(CellTypeInfo info) {
        string formName = info.CellForm == null ? GetRandomFigure() : info.CellForm.FormName;
        bool[,] cells = TetrisPieces.PieceShapesTable[formName];
        Guid[,] cellGuids = new Guid[cells.GetLength(0), cells.GetLength(1)];
        for (int x = 0; x < cells.GetLength(0); x++) {
            for (int y = 0; y < cells.GetLength(1); y++) {
                if (cells[x, y]) {
                    cellGuids[x, y] = Guid.NewGuid();
                } else {
                    cellGuids[x, y] = Guid.Empty;
                }
            }
        }

        var data = new PieceData() {
            Type = info,
            Cells = cells,
            CellGuids = cellGuids,
            FormName = formName
        };
        return data;
    }

    private static PieceData GetNewPiece(List<CellType> cellsToSpawn, float[] chancesToSpawn, CellTypeInfo guaranteed) {
        CellTypeInfo cellInfo = null;

        if (guaranteed != null) {
            cellInfo = guaranteed;
        } else {
            float chance = Random.Range(0, chancesToSpawn[chancesToSpawn.Length - 1]);
            for (int i = 0; i < chancesToSpawn.Length; i++) {
                if (chancesToSpawn[i] > chance) {
                    
                    cellInfo = PiecesViewTable.Instance.CellsList.Combined().First(c => c.CellType == cellsToSpawn[i]);
                    break;
                }
            }
        }

        return GetExactPiece(cellInfo);
    }

    private static string GetRandomFigure() {
        bool isMetaGame = GameFieldManager.Instance == null;
        var chancesToSpawn = isMetaGame ? MetaFieldManager.Instance.FiguresChanceToSpawn : GameFieldManager.Instance.FiguresChanceToSpawn;
        float chance = Random.Range(0, chancesToSpawn[chancesToSpawn.Length - 1]);
        var figureForms = PiecesViewTable.Instance.FigureForms;
        for (int i = 0; i < chancesToSpawn.Length; i++) {
            if (chancesToSpawn[i] > chance)
                return figureForms[i].FormName;
        }

        return null;
    }

    public static bool CanPlacePiece(CellType[,] field, PieceData piece) {
        int fieldWidth = field.GetLength(0);
        int fieldHeight = field.GetLength(1);
        int pieceWidth = piece.Cells.GetLength(0);
        int pieceHeight = piece.Cells.GetLength(1);

        for (int x = 0; x <= fieldWidth - pieceWidth; x++) {
            for (int y = 0; y <= fieldHeight - pieceHeight; y++) {
                if (FieldUtils.CanPlacePiece(field, piece, new Vector2Int(x, y))) {
                    return true;
                }
            }
        }

        return false;
    }
    
    public static List<Vector2Int> CanPlacedPiece(CellType[,] field, PieceData piece) {
        int fieldWidth = field.GetLength(0);
        int fieldHeight = field.GetLength(1);
        int pieceWidth = piece.Cells.GetLength(0);
        int pieceHeight = piece.Cells.GetLength(1);

        for (int x = 0; x <= fieldWidth - pieceWidth; x++) {
            for (int y = 0; y <= fieldHeight - pieceHeight; y++) {
                if (FieldUtils.CanPlacePiece(field, piece, new Vector2Int(x, y))) {
                    return FieldUtils.PlacedPieceCells(field, piece, new Vector2Int(x, y));
                }
            }
        }

        return null;
    }
    
    public static List<Vector2Int> CanPlacedPieceWhenDestroyCells(CellType[,] field, PieceData piece) {
        int fieldWidth = field.GetLength(0);
        int fieldHeight = field.GetLength(1);
        int pieceWidth = piece.Cells.GetLength(0);
        int pieceHeight = piece.Cells.GetLength(1);

        List<List<Vector2Int>> positionsToPlacePiece = new List<List<Vector2Int>>();
        
        for (int x = 0; x <= fieldWidth - pieceWidth; x++) {
            for (int y = 0; y <= fieldHeight - pieceHeight; y++) {
                if (FieldUtils.CanDestroyCellsAndPlacePiece(field, piece, new Vector2Int(x, y))) {
                    positionsToPlacePiece.Add(FieldUtils.PlacedPieceCellsWithoutResource(field, piece, new Vector2Int(x, y))); 
                }
            }
        }

        return positionsToPlacePiece[Random.Range(0, positionsToPlacePiece.Count)];
    }

    private static bool CanPlaceAt(CellType[,] field, bool[,] piece, int offsetX, int offsetY) {
        int pieceWidth = piece.GetLength(0);
        int pieceHeight = piece.GetLength(1);

        for (int x = 0; x < pieceWidth; x++) {
            for (int y = 0; y < pieceHeight; y++) {
                if (!piece[x, y]) {
                    continue;
                }

                var curType = field[offsetX + x, offsetY + y];
                if (curType != CellType.Empty && curType != CellType.Ice) {
                    return false;
                }
            }
        }

        return true;
    }
}