using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartFieldCustomRedactor : MonoBehaviour {
    private RedactorFieldButton[,] _fieldButtons;
    private RedactorMetaFieldButton[,] _metaFieldButtons;
    [SerializeField]
    private MetaStartLockedCellsFieldConfig _metaStartLockedCellsFieldConfig;
    
    private CellType[,] _fieldCellTypes;
    private int[,] _metaFieldLockedCellTypes;
    
    public Button ButtonPrefab;
    
    public RedactorFieldButton FieldButtonPrefab;
    public RedactorMetaFieldButton MetaFieldButtonPrefab;
    public int GameFieldSize;
    public int MetaFieldSize;
    
    public CellType[] CellTypesToRedactor;
    public int[] MetaCellTypesToRedactor;
    
    public CellType CurrentCellType;
    public int CurrentMetaCellType;
    
    public TMP_Text CurrentCellTypeText;
    public Transform ChooseCellTypeButtonsContainer;
    public RectTransform FieldRect;
    public TMP_InputField FieldInput;
    private string _filePath = "Assets/Configs/StartMapConfigs";

    private List<TMP_InputField> _metaCellsParents = new();

    [SerializeField]
    private Toggle _isMetaFieldToggle;

    [SerializeField]
    private Transform _metaCellsParentsContainer;
    
    [SerializeField]
    private TMP_InputField _metaCellsInputPrefab;

    public void Start() {
        int[,] startMetaField = new int[12, 12];
        foreach (var VARIABLE in _metaStartLockedCellsFieldConfig.LockedCellsGroups) {
            startMetaField[VARIABLE.position.x, VARIABLE.position.y] = VARIABLE.index;
        }
string newString = "";
        for (int i = 0; i < startMetaField.GetLength(0); i++) {
            
            for (int j = 0; j < startMetaField.GetLength(1); j++) {
                newString += startMetaField[i, j] + " ,";
            }

            newString += "\n";
        }
         Debug.Log(newString);
        SetRedactButtons();
        SetFieldButtons();
        SetMetaCellsParents();
        _isMetaFieldToggle.onValueChanged.AddListener(ChangeFieldRedactorState);
    }

    private void SetMetaCellsParents() {
        foreach (TMP_InputField input in _metaCellsParents) {
            Destroy(input.gameObject);
        }
        _metaCellsParents.Clear();
        
        for (int i = 0; i < MetaCellTypesToRedactor.Length; i++) {
            _metaCellsParents.Add(Instantiate(_metaCellsInputPrefab, _metaCellsParentsContainer));
            _metaCellsParents[i].text = "1";
        }
    }

    private void SetRedactButtons()
    {
       /* int childCount = ChooseCellTypeButtonsContainer.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(ChooseCellTypeButtonsContainer.GetChild(0).gameObject);
        }*/
        
        foreach (Transform child in ChooseCellTypeButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        if (!_isMetaFieldToggle.isOn)
        {
            foreach (var curCellType in CellTypesToRedactor)
            {
                var buttonPrefab = Instantiate(ButtonPrefab, ChooseCellTypeButtonsContainer);
                buttonPrefab.onClick.AddListener(() => ChangeCurrentCellType(curCellType));
                buttonPrefab.GetComponentInChildren<TMP_Text>().text = curCellType.ToString();
            }
        }
        else
        {
            foreach (var curCellType in MetaCellTypesToRedactor)
            {
                var buttonPrefab = Instantiate(ButtonPrefab, ChooseCellTypeButtonsContainer);
                buttonPrefab.onClick.AddListener(() => ChangeCurrentMetaCellType(curCellType));
                buttonPrefab.GetComponentInChildren<TMP_Text>().text = curCellType.ToString();
            }
        }
    }

    public void ChangeFieldRedactorState(bool isOn)
    {
        SetRedactButtons();
        SetFieldButtons();
    }
    
    public void SetFieldButtons()
    {
        _metaCellsParentsContainer.gameObject.SetActive(_isMetaFieldToggle.isOn);
        foreach (Transform child in FieldRect)
        {
            Destroy(child.gameObject);
        }


        SetButtonsPosition();
    }

    private void SetButtonsPosition()
    {
        int needSize = _isMetaFieldToggle.isOn ? MetaFieldSize : GameFieldSize;
        
        _fieldButtons = new RedactorFieldButton[needSize, needSize];
        _metaFieldButtons = new RedactorMetaFieldButton[needSize, needSize];
        _fieldCellTypes = new CellType[needSize, needSize];
        _metaFieldLockedCellTypes = new int[needSize, needSize];
        for (int i = 0; i < needSize; i++)
        {
            for (int j = 0; j < needSize; j++)
            {
                _metaFieldLockedCellTypes[i, j] = 1;
            }
        }

        
        var yOffset = FieldRect.rect.height / needSize;
        var xOffset = FieldRect.rect.width / needSize;
        for (int i = 0; i < needSize; i++) {
            for (int j = 0; j < needSize; j++) {
                Vector2 butPos = new Vector2(yOffset * (j + 0.5f),xOffset * (i + 0.5f));

                if (_isMetaFieldToggle.isOn)
                    SetMetaFieldButton(butPos, i, j);
                else
                    SetGameFieldButton(butPos, i, j);
            }
        }
    }

    private void SetGameFieldButton(Vector2 butPos, int i, int j)
    { 
        var button = Instantiate(FieldButtonPrefab, FieldRect);
        button.transform.localPosition = butPos;
        button.SetData(new Vector2Int(i, j), ChangeCellCellType);
        button.SetType(CellType.Empty);
        _fieldButtons[i, j] = button;
    }
    
    private void SetMetaFieldButton(Vector2 butPos, int i, int j)
    { 
        var button = Instantiate(MetaFieldButtonPrefab, FieldRect);
        button.transform.localPosition = butPos;
        button.SetData(new Vector2Int(i, j), ChangeMetaCellCellType);
        button.SetType(1);
        _metaFieldButtons[i, j] = button;
    }

    public void ChangeCurrentCellType(CellType cellType) {
        CurrentCellType = cellType;
        CurrentCellTypeText.text = cellType.ToString();
    }
    
    public void ChangeCurrentMetaCellType(int cellType) {
        CurrentMetaCellType = cellType;
        CurrentCellTypeText.text = cellType.ToString();
    }

    public void ChangeCellCellType(Vector2Int coord) {
     
        _fieldCellTypes[coord.x, coord.y] = CurrentCellType;
        _fieldButtons[coord.x, coord.y].SetType(_fieldCellTypes[coord.x, coord.y]);
    }
    
    public void ChangeMetaCellCellType(Vector2Int coord) {
      
        _metaFieldLockedCellTypes[coord.y, coord.x] = CurrentMetaCellType;
        _metaFieldButtons[coord.x, coord.y].SetType(_metaFieldLockedCellTypes[coord.y, coord.x]);
    }

    public void ClearField() {
        if (_isMetaFieldToggle.isOn)
        {
            for (int i = 0; i < GameFieldSize; i++)
            {
                for (int j = 0; j < GameFieldSize; j++)
                {
                    _metaFieldLockedCellTypes[i, j] = 1;
                    _metaFieldButtons[i, j].SetType(1);
                }
            }
        }
        else
        {
            SetMetaCellsParents();
            for (int i = 0; i < GameFieldSize; i++)
            {
                for (int j = 0; j < GameFieldSize; j++)
                {
                    _fieldCellTypes[i, j] = CellType.Empty;
                    _fieldButtons[i, j].SetType(CellType.Empty);
                }
            }
        }
    }

#if UNITY_EDITOR

    public void LoadConfigButton() {
        if (FieldInput.text == "") return;
        
        string assetPath = _filePath + "/" + FieldInput.text + ".asset";
        print(assetPath);
        
        if (_isMetaFieldToggle.isOn) {
            SetMetaCellsParents();
            MetaStartLockedCellsFieldConfig config = AssetDatabase.LoadAssetAtPath<MetaStartLockedCellsFieldConfig>(assetPath);
            foreach (IntAndVector2Int cell in config.LockedCellsGroups) {
                _metaFieldLockedCellTypes[cell.position.x, cell.position.y] = cell.index;
                _metaFieldButtons[cell.position.x, cell.position.y].SetType(cell.index);
            }

            for (int i = 0; i < config.GroupsParents.Count; i++) {
                _metaCellsParents[i].text = config.GroupsParents[i].ToString();
            }
        }
        else
        {
            StartFieldConfig config = AssetDatabase.LoadAssetAtPath<StartFieldConfig>(assetPath);
            for (int i = 0; i < config.Grid.Count; i++) {
                for (int j = 0; j < config.Grid[i].row.Count; j++) {
                    _fieldCellTypes[i, j] = config.Grid[i].row[j];
                    _fieldButtons[i, j].SetType(config.Grid[i].row[j]);
                }
            }
        }
    }
    
    public void SaveConfigButton()
    {
        if (_isMetaFieldToggle.isOn)
            SaveStartMetaFieldToConfig();
        else
            SaveStartGameFieldToConfig();
    }

    public void PlayButton() {
        SceneManager.LoadScene("LoadingScene");
    }
    
    public void AutofillLevels() {
        AutoFillLevelConfigs();
    }
    
    [MenuItem("Tools/Configs/Auto-Fill Levels in MainManagerConfig")]
    public static void AutoFillLevelConfigs()
    {
        // Находим MainManagerConfig в проекте
        var mainManagerConfigs = AssetDatabase.FindAssets("t:MainManagerConfig");
        if (mainManagerConfigs.Length == 0)
        {
            Debug.LogError("MainManagerConfig not found in project!");
            return;
        }

        var mainManagerConfig = AssetDatabase.LoadAssetAtPath<MainManagerConfig>(
            AssetDatabase.GUIDToAssetPath(mainManagerConfigs[0]));

        // Находим все LevelConfig в проекте
        var levelConfigs = AssetDatabase.FindAssets("t:LevelConfig")
            .Select(guid => AssetDatabase.LoadAssetAtPath<LevelConfig>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(config => config != null)
            .ToList();

        // Сортируем по имени (Level33Config, Level34Config и т.д.)
        // Сортируем по номеру уровня, извлекая число из имени
        levelConfigs.Sort((a, b) => 
        {
            int aNumber = ExtractNumber(a.name);
            int bNumber = ExtractNumber(b.name);
            return aNumber.CompareTo(bNumber);
        });

        // Присваиваем отсортированный массив
        mainManagerConfig.Levels = levelConfigs.ToArray();

        // Помечаем объект как измененный, чтобы сохранить изменения
        EditorUtility.SetDirty(mainManagerConfig);
        AssetDatabase.SaveAssets();

        Debug.Log($"Successfully assigned {levelConfigs.Count} level configs to MainManagerConfig in order: " +
                  string.Join(", ", levelConfigs.Select(c => c.name)));
    }
    
    private static int ExtractNumber(string name)
    {
        // Ищем последовательность цифр в имени
        Match match = Regex.Match(name, @"\d+");
        if (match.Success && int.TryParse(match.Value, out int number))
        {
            return number;
        }
        return 0; // Если числа не найдены, возвращаем 0
    }
    
    private void SaveStartMetaFieldToConfig() {
        if (FieldInput.text == "") return;
        MetaStartLockedCellsFieldConfig config = ScriptableObject.CreateInstance<MetaStartLockedCellsFieldConfig>();
        config.CreateGrid(_metaFieldLockedCellTypes);
        config.SaveParents(_metaCellsParents);

        string assetPath = _filePath + "/" + FieldInput.text + ".asset";

        MetaStartLockedCellsFieldConfig existingConfig = AssetDatabase.LoadAssetAtPath<MetaStartLockedCellsFieldConfig>(assetPath);

        if (existingConfig != null) {
            existingConfig.CreateGrid(_metaFieldLockedCellTypes);
            existingConfig.SaveParents(_metaCellsParents);
            EditorUtility.SetDirty(existingConfig);
        } else
            AssetDatabase.CreateAsset(config, assetPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private void SaveStartGameFieldToConfig() {
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