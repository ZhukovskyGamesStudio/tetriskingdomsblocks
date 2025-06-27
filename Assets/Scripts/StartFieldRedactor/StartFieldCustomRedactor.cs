using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class StartFieldCustomRedactor : MonoBehaviour {
    private RedactorFieldButton[,] _fieldButtons;
    private CellType[,] _fieldCellTypes;
    public Button ButtonPrefab;
    public RedactorFieldButton FieldButtonPrefab;
    public int FieldSize;
    public CellType[] CellTypesToRedactor;
    public CellType CurrentCellType;
    public TMP_Text CurrentCellTypeText;
    public Transform ChooseCellTypeButtonsContainer;
    public RectTransform FieldRect;
    public TMP_InputField FieldInput;
    private string _filePath = "Assets/Configs/StartMapConfigs";

    public void Start() {
        SetRedactButtons();
        SetFieldButtons();
    }

    private void SetRedactButtons() {
        foreach (var curCellType in CellTypesToRedactor) {
            var buttonPrefab = Instantiate(ButtonPrefab, ChooseCellTypeButtonsContainer);
            buttonPrefab.onClick.AddListener(() => ChangeCurrentCellType(curCellType));
            buttonPrefab.GetComponentInChildren<TMP_Text>().text = curCellType.ToString();
        }
    }

    public void SetFieldButtons() {
        _fieldButtons = new RedactorFieldButton[FieldSize, FieldSize];
        _fieldCellTypes = new CellType[FieldSize, FieldSize];
        var yOffset = FieldRect.rect.height / FieldSize;
        var xOffset = FieldRect.rect.width / FieldSize;
        for (int i = 0; i < FieldSize; i++) {
            for (int j = 0; j < FieldSize; j++) {
                Vector2 butPos = new Vector2(yOffset * (j + 0.5f),xOffset * (i + 0.5f));
                var button = Instantiate(FieldButtonPrefab, FieldRect);
                button.transform.localPosition = butPos;
                button.SetData(new Vector2Int(i, j), ChangeCellCellType);
                button.SetType(CellType.Empty);
                _fieldButtons[i, j] = button;
            }
        }
    }

    public void ChangeCurrentCellType(CellType cellType) {
        CurrentCellType = cellType;
        CurrentCellTypeText.text = cellType.ToString();
    }

    public void ChangeCellCellType(Vector2Int coord) {
        Debug.Log($"ChangeCellCellType({coord.x}, {coord.y})");
        _fieldCellTypes[coord.x, coord.y] = CurrentCellType;
        _fieldButtons[coord.x, coord.y].SetType(_fieldCellTypes[coord.x, coord.y]);
    }

    public void ClearField() {
        for (int i = 0; i < FieldSize; i++) {
            for (int j = 0; j < FieldSize; j++) {
                _fieldCellTypes[i, j] = CellType.Empty;
                _fieldButtons[i, j].SetType(CellType.Empty);
            }
        }
    }

#if UNITY_EDITOR
    public void SaveToConfig() {
        if (FieldInput.text == "") return;
        StartFieldConfig config = ScriptableObject.CreateInstance<StartFieldConfig>();
        config.CreateGrid(_fieldCellTypes);

        string assetPath = _filePath + "/" + FieldInput.text + ".asset";

        StartFieldConfig existingConfig = AssetDatabase.LoadAssetAtPath<StartFieldConfig>(assetPath);

        if (existingConfig != null) {
            existingConfig.CreateGrid(_fieldCellTypes);
            EditorUtility.SetDirty(existingConfig);
        } else
            AssetDatabase.CreateAsset(config, assetPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}