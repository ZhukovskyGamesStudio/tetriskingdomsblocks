using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartFieldConfig", menuName = "Scriptable Objects/StartFieldConfig")]
public class StartFieldConfig : ScriptableObject
{
    [SerializeField] private List<CellRow> grid = new List<CellRow>();

    // Для удобного доступа к элементам
    public CellType GetCell(int x, int y)
    {
        if (y >= 0 && y < grid.Count && x >= 0 && x < grid[y].row.Count)
        {
            return grid[y].row[x];
        }
        return CellType.Empty; // или выбросить исключение
    }

    public void SetCell(int x, int y, CellType value)
    {
        if (y >= 0 && y < grid.Count && x >= 0 && x < grid[y].row.Count)
        {
            grid[y].row[x] = value;
        }
    }

    // Для отображения в инспекторе
    [System.Serializable]
    public class CellRow
    {
        public List<CellType> row = new List<CellType>();
    }
}
