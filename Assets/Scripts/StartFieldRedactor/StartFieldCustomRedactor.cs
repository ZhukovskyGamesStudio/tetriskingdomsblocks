using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class StartFieldCustomRedactor : MonoBehaviour
{
    private TMP_Text[,] _fieldTexts;
    private CellType[,] _fieldCellTypes;
    public Button ButtonPrefab;
    public int FieldSize;
    public CellType[] CellTypesToRedactor;
    public CellType CurrentCellType;
    public TMP_Text CurrentCellTypeText;
    public Transform ChooseCellTypeButtonsContainer;
    public RectTransform FieldRect;
    public TMP_InputField FieldInput;
    private string _filePath = "Assets/Configs/StartMapConfigs";

    public void Start()
    {
        SetRedactButtons();
        SetFieldButtons();
    }

    private void SetRedactButtons()
    {
        foreach (var curCellType in CellTypesToRedactor)
        {
            var buttonPrefab = Instantiate(ButtonPrefab, ChooseCellTypeButtonsContainer);
            buttonPrefab.onClick.AddListener(() => ChangeCurrentCellType(curCellType));
            buttonPrefab.GetComponentInChildren<TMP_Text>().text = curCellType.ToString();
        }
    }

    public void SetFieldButtons()
    {
        _fieldTexts = new TMP_Text[FieldSize, FieldSize];
        _fieldCellTypes = new CellType[FieldSize, FieldSize];
        var yOffset = FieldRect.rect.height/FieldSize;
        var xOffset = FieldRect.rect.width/FieldSize;
        for (int i = 0; i < FieldSize; i++)
        {
            for (int j = 0; j < FieldSize; j++)
            {
                Vector2 butPos = new Vector2(xOffset * (i + 0.5f), yOffset * (j + 0.5f));
                var buttonPrefab = Instantiate(ButtonPrefab, FieldRect);
                buttonPrefab.transform.localPosition = butPos;
                int curIndex = i;
                int curSecondIndex = j;
                buttonPrefab.onClick.AddListener(() => ChangeCellCellType(curIndex, curSecondIndex));
                _fieldTexts[i, j] = buttonPrefab.GetComponentInChildren<TMP_Text>();
                _fieldTexts[i, j].text = CellType.Empty.ToString();
            }
        }
    }

    public void ChangeCurrentCellType(CellType cellType)
    {
        CurrentCellType = cellType;
        CurrentCellTypeText.text = cellType.ToString();
    }

    public void ChangeCellCellType(int row,int col)
    {
        Debug.Log($"ChangeCellCellType({row}, {col})");
        _fieldCellTypes[row,col] = CurrentCellType;
        _fieldTexts[row,col].text = _fieldCellTypes[row,col].ToString();
    }
#if UNITY_EDITOR
    public void SaveToConfig()
    {
        if(FieldInput.text == "")return;
        StartFieldConfig config = ScriptableObject.CreateInstance<StartFieldConfig>();
        config.CreateGrid(_fieldCellTypes);
        string assetPath = _filePath + "/"+FieldInput.text+".asset";
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}
