using System;

[Serializable]
public class PieceData {
    public bool[,] Cells;
    public CellTypeInfo Type;
    public Guid[,] CellGuids;
}