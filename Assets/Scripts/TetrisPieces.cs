using System.Collections.Generic;

public static class TetrisPieces {
    public static List<bool[,]> PieceShapesTable = new List<bool[,]> {
        // I
        new bool[,] {
            { true, true, true, true }
        },

        // O
        new bool[,] {
            { true, true },
            { true, true }
        },

        // T
        new bool[,] {
            { true, true, true },
            { false, true, false }
        },

        // S
        new bool[,] {
            { false, true, true },
            { true, true, false }
        },

        // Z
        new bool[,] {
            { true, true, false },
            { false, true, true }
        },

        // J
        new bool[,] {
            { true, false, false },
            { true, true, true }
        },

        // L
        new bool[,] {
            { false, false, true },
            { true, true, true }
        }
    };
}