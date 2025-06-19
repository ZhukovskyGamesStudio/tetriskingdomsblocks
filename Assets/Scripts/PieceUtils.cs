using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class PieceUtils {
    public static PieceData GetNewPiece()
    {
        bool isMetaGame = GameManager.Instance == null;
        var cellsToSpawn = !isMetaGame ? GameManager.Instance._currentCellsToSpawn : MetaManager.Instance._currentCellsToSpawn;
        var chancesToSpawn = !isMetaGame ?GameManager.Instance.CellsChanceToSpawn: MetaManager.Instance.CellsChanceToSpawn;
        CellTypeInfo cellInfo = null;
        float chance = Random.Range(0, chancesToSpawn[chancesToSpawn.Length - 1]);
        for (int i = 0; i < chancesToSpawn.Length; i++)
        {
            if (chancesToSpawn[i] > chance)
            {
                cellInfo = cellsToSpawn[i];
                break;
            }
        }
        
        if (!isMetaGame && GameManager.Instance.CurrentGuaranteedFirstCells.Count != 0)
        {
            cellInfo = GameManager.Instance.CurrentGuaranteedFirstCells[0];
            GameManager.Instance.CurrentGuaranteedFirstCells.RemoveAt(0);
        }

        bool[,] cells = cellInfo.CellForm == null ? GetRandomFigure() : TetrisPieces.PieceShapesTable[cellInfo.CellForm.FormName];
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
            Type = cellInfo,
            Cells = cells,
            CellGuids = cellGuids
        };
        return data;
    }

    public static bool[,] GetRandomFigure()
    {
        bool isMetaGame = GameManager.Instance == null;
        var chancesToSpawn = isMetaGame ? MetaManager.Instance.FiguresChanceToSpawn:GameManager.Instance.FiguresChanceToSpawn;
        float chance = Random.Range(0, chancesToSpawn[chancesToSpawn.Length - 1]);
        var figureForms = isMetaGame ? MetaManager.Instance.FigureFormsConfig:GameManager.Instance.FigureFormsConfig;
        for (int i = 0; i < chancesToSpawn.Length; i++)
        {
            if (chancesToSpawn[i] > chance)
                return TetrisPieces.PieceShapesTable[figureForms[i].FormName];
        }

        return null;
    }
    public static bool CanPlacePiece(CellType[,] field, bool[,] piece) {
        int fieldWidth = field.GetLength(0);
        int fieldHeight = field.GetLength(1);
        int pieceWidth = piece.GetLength(0);
        int pieceHeight = piece.GetLength(1);

        for (int x = 0; x <= fieldWidth - pieceWidth; x++) {
            for (int y = 0; y <= fieldHeight - pieceHeight; y++) {
                if (CanPlaceAt(field, piece, x, y)) {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool CanPlaceAt(CellType[,] field, bool[,] piece, int offsetX, int offsetY) {
        int pieceWidth = piece.GetLength(0);
        int pieceHeight = piece.GetLength(1);

        for (int x = 0; x < pieceWidth; x++) {
            for (int y = 0; y < pieceHeight; y++) {
                if (!piece[x, y]) {
                    continue;
                }

                if (field[offsetX + x, offsetY + y] != CellType.Empty) {
                    return false;
                }
            }
        }

        return true;
    }
}