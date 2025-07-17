using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CellsViewList", menuName = "Scriptable Objects/CellsViewList", order = 0)]
public class CellsViewList : ScriptableObject {
    [field: SerializeField]
    public List<CellView> CellsViews;

    public CellView GetCellByType(CellType type) {
        Debug.Log("GetCellByType"+type);
        return CellsViews.First(c => c.CellType == type);
    }
}