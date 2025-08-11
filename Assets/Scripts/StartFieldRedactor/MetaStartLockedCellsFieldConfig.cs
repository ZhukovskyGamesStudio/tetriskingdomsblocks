using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "MetaStartLockedCellsFieldConfig", menuName = "Scriptable Objects/MetaStartLockedCellsFieldConfig")]
public class MetaStartLockedCellsFieldConfig : ScriptableObject
{
    public IntAndVector2Int[] LockedCellsGroups ;
    public List<int> GroupsParents;

    public void SaveParents(List<TMP_InputField> inputs) {
        GroupsParents = new List<int>();
        foreach (TMP_InputField input in inputs) {
            GroupsParents.Add(int.Parse(input.text));
        }
    }
    
    public void CreateGrid(int[,] inputGrid)
    {
        LockedCellsGroups = new IntAndVector2Int[inputGrid.GetLength(0)* inputGrid.GetLength(1)];
        
        int rows = inputGrid.GetLength(0);
        int cols = inputGrid.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                LockedCellsGroups[i * rows+j] = (new IntAndVector2Int(new Vector2Int(i, j),inputGrid[i, j]));
            }
            
        }
    }
}
[System.Serializable]
public struct IntAndVector2Int
{
    public Vector2Int position;
    public int index;

    public IntAndVector2Int(Vector2Int position, int index)
    {
        this.position = position;
        this.index = index;
    }
}
