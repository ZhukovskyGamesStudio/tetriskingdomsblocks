using System.Collections.Generic;

public static class TetrisPieces
{
    public static List<bool[,]> PieceShapesTable = new List<bool[,]> {
        // big I horizontal
        new bool[,] {
            { true, true, true, true }
        },

        // big I
         new bool[,] {
            { true},
              { true},
               { true},
            { true}
        },

          // I horizontal
        new bool[,] {
            { true, true, true}
        },
        // I 
        new bool[,] {
            { true},
              { true},
            { true}
        },
          // , horizontal
        new bool[,] {
            { true, true}
        },

         // ,
         new bool[,] {
            { true},
            { true}
        },

          // .
        new bool[,] {
            { true}
        },

        // O
        new bool[,] {
            { true, true },
            { true, true }
        },

         // big O
        new bool[,] {
            { true, true ,true},
            { true, true ,true},
            { true, true ,true}
        },

        // +
         new bool[,] {
            { false, true ,false},
            { true, true ,true},
            { false, true ,false}
        },

           // donut
        new bool[,] {
            { true, true ,true},
            { true, false ,true},
            { true, true ,true}
        },

        //rainbow
          new bool[,] {
            { true, true ,true},
            { true, false ,true},
            { true, false ,true}
        },

             //smile
          new bool[,] {
            { true, false ,true},
            { true, false ,true},
            { true, true ,true}
        },

             // corner left down
        new bool[,] {
            { true, false },
            { true, true }
        },
          // corner right down
        new bool[,] {
            { false, true },
            { true, true }
        },
          // corner left up
        new bool[,] {
            { true, true },
            { true, false }
        },
          // corner right up
        new bool[,] {
            { true, true },
            { false, true }
        },

        // T small
        new bool[,] {
            { true, true, true },
            { false, true, false }
        },

        // , .
        new bool[,] {
            { true, false, false },
            { true, false, true }
        },

           // , . vertical
        new bool[,] {
            { true, false},
            { false, false},
            { true, true}
        },

        // T
         new bool[,] {
            { true, true, true },
            { false, true, false },
             { false, true, false }
        },

         // T rotated
         new bool[,] {
            { true, false, false },
            { true, true, true },
             { true, false, false }
        },

        // S
        new bool[,] {
            { false, true, true },
            { true, true, false }
        },

        // S rotated
        new bool[,] {
            { true, false},
            { true, true},
            { false, true}
        },

        // Z
        new bool[,] {
            { true, true, false },
            { false, true, true }
        },

         // Z rotated
        new bool[,] {
            { false, true},
            { true, true},
            { true, false}
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
        },

        //U
         new bool[,] {
            { true, false, true },
            { true, true, true }
        }
         ,

        //U rotated
         new bool[,] {
            { true, true, true },
            { true, false, true }
        }
    };
}