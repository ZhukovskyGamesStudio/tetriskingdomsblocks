using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(StartFieldConfig))]
public class StartFieldConfigEditor : Editor
{
    private const int CellSize = 20;
    private const int Padding = 2;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var config = (StartFieldConfig)target;
        var gridField = typeof(StartFieldConfig).GetField("grid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var grid = gridField?.GetValue(config) as List<StartFieldConfig.CellRow>;
        if (grid == null || grid.Count == 0)
        {
            EditorGUILayout.HelpBox("Поле пустое", MessageType.Info);
            return;
        }
        int rows = grid.Count;
        int cols = 0;
        foreach (var row in grid)
        {
            if (row != null && row.row.Count > cols)
                cols = row.row.Count;
        }
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Визуализация поля:", EditorStyles.boldLabel);
        Rect rect = GUILayoutUtility.GetRect(cols * (CellSize + Padding), rows * (CellSize + Padding));
        rect.height = rows * (CellSize + Padding);
        rect.width = cols * (CellSize + Padding);
        for (int y = 0; y < rows; y++)
        {
            var row = grid[y];
            for (int x = 0; x < cols; x++)
            {
                Color color = Color.gray;
                if (row != null && x < row.row.Count)
                {
                    var cellType = row.row[x];
                    if (RedactorFieldButton.Colors.TryGetValue(cellType, out var c))
                        color = c;
                }
                Rect cellRect = new Rect(
                    rect.x + x * (CellSize + Padding),
                    rect.y + (rows-1-y) * (CellSize + Padding),
                    CellSize, CellSize);
                EditorGUI.DrawRect(cellRect, color);
            }
        }
        GUILayout.Space(rows * (CellSize + Padding));
    }
} 