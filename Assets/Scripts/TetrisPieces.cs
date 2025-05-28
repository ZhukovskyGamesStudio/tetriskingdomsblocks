using System.Collections.Generic;

public static class TetrisPieces
{
    //change to dictionary with key figureForm(new enum)
    public static Dictionary<string,bool[,]> PieceShapesTable = new() {
        // big I horizontal
        {
            "bigIhorizontal", new bool[,]
            {
                { true, true, true, true }
            }
        },

        // big I
        {
            "bigI", new bool[,]
            {
                { true },
                { true },
                { true },
                { true }
            }
        },

        // I horizontal
        {
            "Ihorizontal", new bool[,]
            {
                { true, true, true }
            }
        },
        // I 
        {
            "I", new bool[,]
            {
                { true },
                { true },
                { true }
            }
        },
        // , horizontal
        {
            "smallIhorizontal", new bool[,]
            {
                { true, true }
            }
        },

        // ,
        {
            "smallI", new bool[,]
            {
                { true },
                { true }
            }
        },

        // .
        {
            "oneBlock", new bool[,]
            {
                { true }
            }
        },

        // O
        {
            "smallSquare", new bool[,]
            {
                { true, true },
                { true, true }
            }
        },

        // big O
        {
            "bigSquare", new bool[,]
            {
                { true, true, true },
                { true, true, true },
                { true, true, true }
            }
        },

        // +
        {
            "plus", new bool[,]
            {
                { false, true, false },
                { true, true, true },
                { false, true, false }
            }
        },

        // donut
        {
            "donut", new bool[,]
            {
                { true, true, true },
                { true, false, true },
                { true, true, true }
            }
        },

        //rainbow
        {
            "rainbow", new bool[,]
            {
                { true, true, true },
                { true, false, true },
                { true, false, true }
            }
        },

        //smile
        {
            "smile", new bool[,]
            {
                { true, false, true },
                { true, false, true },
                { true, true, true }
            }
        },

        // corner left down
        {
            "cornerLeftDown", new bool[,]
            {
                { true, false },
                { true, true }
            }
        },
        // corner right down
        {
            "cornerRightDown", new bool[,]
            {
                { false, true },
                { true, true }
            }
        },
        // corner left up
        {
            "cornerLeftUp", new bool[,]
            {
                { true, true },
                { true, false }
            }
        },
        // corner right up
        {
            "cornerRightUp", new bool[,]
            {
                { true, true },
                { false, true }
            }
        },

        // T small
        {
            "TSmall", new bool[,]
            {
                { true, true, true },
                { false, true, false }
            }
        },

        // , .
        {
            "oneAndTwoHorizontal", new bool[,]
            {
                { true, false, false },
                { true, false, true }
            }
        },

        // , . vertical
        {
            "oneAndTwo", new bool[,]
            {
                { true, false },
                { false, false },
                { true, true }
            }
        },

        // T
        {
            "T", new bool[,]
            {
                { true, true, true },
                { false, true, false },
                { false, true, false }
            }
        },

        // T rotated
        {
            "TRotated", new bool[,]
            {
                { true, false, false },
                { true, true, true },
                { true, false, false }
            }
        },

        // S
        {
            "S", new bool[,]
            {
                { false, true, true },
                { true, true, false }
            }
        },

        // S rotated
        {
            "SRotated", new bool[,]
            {
                { true, false },
                { true, true },
                { false, true }
            }
        },

        // Z
        {
            "Z", new bool[,]
            {
                { true, true, false },
                { false, true, true }
            }
        },

        // Z rotated
        {
            "ZRotated", new bool[,]
            {
                { false, true },
                { true, true },
                { true, false }
            }
        },

        // J
        {
            "J", new bool[,]
            {
                { true, false, false },
                { true, true, true }
            }
        },

        // L
        {
            "L", new bool[,]
            {
                { false, false, true },
                { true, true, true }
            }
        },

        //U
        {
            "U", new bool[,]
            {
                { true, false, true },
                { true, true, true }
            }
        },

        //U rotated
        {
            "URotated", new bool[,]
            {
                { true, true, true },
                { true, false, true }
            }
        }
    };
}