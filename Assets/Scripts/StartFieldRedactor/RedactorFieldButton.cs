using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RedactorFieldButton : MonoBehaviour {
    private Action<Vector2Int> _onChange;
    private Vector2Int _coord;

    [SerializeField]
    private TextMeshProUGUI _nameText;

    [SerializeField]
    private Image _background;

    public void SetData(Vector2Int coord, Action<Vector2Int> onChange) {
        _coord = coord;
        _onChange = onChange;
    }

    public void SetType(CellType type) {
        _nameText.text = type.ToString();
        _background.color = Colors[type];
    }

    public void OnChange() {
        _onChange?.Invoke(_coord);
    }

    public void OnChangeWithMouseCheck() {
        if (!Input.GetMouseButton(0)) {
            return;
        }

        OnChange();
    }

    public static readonly Dictionary<CellType, Color> Colors = new Dictionary<CellType, Color>() {
        { CellType.Empty, Color.white },
        { CellType.Box, Color.red },
        { CellType.Ice, Color.blue },
        { CellType.GoldMine, Color.yellow },
        { CellType.CrystalMine, Color.magenta },
        { CellType.Slime, Color.green },
        { CellType.Wood, Color.green },
        { CellType.Stone, Color.grey },
        { CellType.LockedMetaCell, Color.blue },
    };
}