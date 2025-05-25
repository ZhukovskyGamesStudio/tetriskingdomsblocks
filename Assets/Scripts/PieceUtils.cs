using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class PieceUtils {
    public static PieceData GetNewPiece() {
        var l = Enum.GetNames(typeof(CellType));
        var data = new PieceData() {
            Type = Enum.Parse<CellType>(l[Random.Range(1, l.Length)]),
            Cells = TetrisPieces.PieceShapesTable[Random.Range(0, TetrisPieces.PieceShapesTable.Count)]
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