using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FigureFormConfig))]
public class FigureFormConfigEditor : Editor
{
    private const int CellSize = 20;
    private const int Padding = 2;
    private static readonly Color FilledColor = new Color(0.2f, 0.7f, 1f, 1f);
    private static readonly Color EmptyColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var config = (FigureFormConfig)target;
        if (string.IsNullOrEmpty(config.FormName))
        {
            EditorGUILayout.HelpBox("FormName не задан", MessageType.Info);
            return;
        }
        if (!TetrisPieces.PieceShapesTable.TryGetValue(config.FormName, out var shape))
        {
            EditorGUILayout.HelpBox($"Форма '{config.FormName}' не найдена в TetrisPieces.PieceShapesTable", MessageType.Warning);
            return;
        }
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Визуализация фигуры:", EditorStyles.boldLabel);
        int rows = shape.GetLength(0);
        int cols = shape.GetLength(1);
        Rect rect = GUILayoutUtility.GetRect(cols * (CellSize + Padding), rows * (CellSize + Padding));
        rect.height = rows * (CellSize + Padding);
        rect.width = cols * (CellSize + Padding);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Rect cellRect = new Rect(
                    rect.x + x * (CellSize + Padding),
                    rect.y + y * (CellSize + Padding),
                    CellSize, CellSize);
                EditorGUI.DrawRect(cellRect, shape[y, x] ? FilledColor : EmptyColor);
            }
        }
        GUILayout.Space(rows * (CellSize + Padding));
    }
} 