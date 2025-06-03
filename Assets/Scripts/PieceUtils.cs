using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class PieceUtils {
    public static PieceData GetNewPiece(CellsAndResourceTypesTableConfig possibleTypes) {
        var l = Enum.GetNames(typeof(CellType));
        var randomShapeType = TetrisPieces.PieceShapesTable.Keys.ToList()[Random.Range(0, TetrisPieces.PieceShapesTable.Keys.Count)];
        var randomPieceShape = TetrisPieces.PieceShapesTable[randomShapeType];
        var data = new PieceData() {
            Type = possibleTypes.cellsToSpawn[Random.Range(0, possibleTypes.cellsToSpawn.Length)],
            Cells = randomPieceShape
        };
        return data;
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