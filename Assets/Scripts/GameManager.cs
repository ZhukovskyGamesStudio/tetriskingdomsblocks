using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IResetable {
    public static GameManager Instance;

    public MainGameConfig MainGameConfig;

    public const int CELL_SIZE = 1;
    private CellTypeInfo[,] _field;
    private List<PieceData> _nextBlocks = new List<PieceData>();
private List<Vector2> _cellsToDestroy = new List<Vector2>();
    //[SerializeField]
   // private TMP_Text _buttonEndGameText;
    
    [SerializeField]
    private Transform _fieldContainer;

    private CellView[,] _cells;

    [SerializeField]
    private Transform _fieldStart, _fieldEnd;

    [SerializeField]
    private TaskUIView[] _taskUIViews;

    [SerializeField]
    private Camera _raycastCamera;

    [SerializeField]
    private TMP_Text helperText;
    
    [SerializeField]
    private Transform _downUITransform;

    [SerializeField]
    private FloatingTextView _floatingTextPrefab;

    [SerializeField]
    private Transform _floatingTextContainer;
    
    [field:SerializeField]
    public Transform CameraContainer;
    
    [SerializeField]
    public RectTransform BgTasksImage;

    [field:SerializeField]
    public Transform _markedCell{ get; private set; }

    [field: SerializeField] 
    public Transform OpenedDoorEndGame;

    
    public List<CellTypeInfo> currentCellsToSpawn;
    [field:SerializeField]
    public float[] CellsChanceToSpawn { get; private set; }

    [field:SerializeField]
    public TMP_Text _mainTextUp { get; private set; }
    
    [SerializeField]
    private LevelConfig _currentLevelConfig;
    
    public SpawnedForOneCharTextView _characterInfoTextHelper;

    private List<TaskInfoAndUI> _currentTasks;

    private Dictionary<ResourceType, int> _monoLinesCount;

    private Dictionary<CellType, int> _placedCellsCount;

    private List<CraftingCellInfo> _currentCraftedCells = new List<CraftingCellInfo>();

    public Vector3 ScreenToWorldPoint => _raycastCamera.ScreenToWorldPoint(Input.mousePosition);
    public Vector3 TouchToWorldPoint => _raycastCamera.ScreenToWorldPoint(Input.GetTouch(0).position);
    private int _placedPiecesAmount;

    private ObjectPool<FloatingTextView> _floatingTextsPool;

    public List<CellTypeInfo> currentGuaranteedFirstCells;
    public GameData GameData { get; private set; }
    private float _screenRatio;

    private Tween _currentTween;

    private void Awake() {
        ChangeToLoading.TryChange();
        Instance = this;
        _floatingTextsPool = new ObjectPool<FloatingTextView>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
        _screenRatio = (float)Screen.width / Screen.height;
        Debug.Log("sr = "+_screenRatio);
    }

    private void Start() {
        Reset();
        Application.targetFrameRate = 144;
        _downUITransform.position = new Vector3(_downUITransform.position.x,_downUITransform.position.y * (_screenRatio / 0.56f), _downUITransform.position.z) ;
      //  Debug.Log(_cameraContainer.position.y + " " + (_screenRatio / 0.45f));
        CameraContainer.position = new Vector3(CameraContainer.position.x,CameraContainer.position.y / (_screenRatio / 0.5f), CameraContainer.position.z) ;
    }

    private void GenerateField() { }

    private void GenerateTask() {
        /*var generatedGoal = TasksUtils.GenerateNewResourceTask();
        GameData.TaskData.GoalToCollect.Add(generatedGoal.Key, generatedGoal.Value);
        GoalView.Instance.InitTask(GameData);*/
    }

    private void ShakeCamera()
    {
        _currentTween.Kill();
        _currentTween = DOTween.Sequence()
            .Append(CameraContainer.transform.DOMoveY(CameraContainer.transform.position.y * 1.02f, 0.12f))
            .Append(CameraContainer.transform.DOMoveY(10f / (_screenRatio / 0.5f), 0.08f));
    }
    private void StartGame() {
        /*
        var startCells = currentLevelConfig.cellTypesTableConfig;

        for (int i = 0; i < startCells.cellsToSpawn.Length; i++)
            currentCellsToSpawn.Add(startCells.cellsToSpawn[i]);
        */
    }

    public void GenerateNewPieces() {
        _nextBlocks = new List<PieceData>() {
            PieceUtils.GetNewPiece(),
            PieceUtils.GetNewPiece(),
            PieceUtils.GetNewPiece()
        };
        NextPiecesView.Instance.SetData(_nextBlocks);
    }

    private void Update() {
        //  Debug.Log(GetPosOnField() + "    Dragshift: " + PieceView.DragShift+ "    Piece: " + GetPieceClampedPosOnField());
    }

    public bool CanPlace(PieceData data) {
        Vector2Int pos = GetPieceClampedPosOnField();
        return CanPlace(data, pos);
    }

    public Vector2Int GetPosOnField() {
        Vector3 coord = GetCoord();
        if (PieceView.PieceMaxSize.x % 2 == 0) {
            coord += Vector3.left / 2f;
        }

        if (PieceView.PieceMaxSize.y % 2 == 0) {
            coord += Vector3.back / 2f;
        }

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        //pos -= new Vector2Int((int)_fieldStart.position.x, (int)_fieldStart.position.z);
        return pos;
    }

    private static Vector3 GetCoord() {
        var coord = Instance.ScreenToWorldPoint;
        //coord += new Vector3(0, 0, 6);
        return coord;
    }

    public Vector2Int GetPieceClampedPosOnField() {
        Vector3 coord = GetCoord();
        coord += new Vector3(PieceView.DragShift.x, 0, PieceView.DragShift.z) + Vector3.forward;
        /*
       if (CalculateShift().x % 1 != 0) {
           DragShift-= Vector3.right/2;
       }
       if (CalculateShift().z % 1 != 0) {
           DragShift-= Vector3.forward/2;
       }*/

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(coord.x) / CELL_SIZE, Mathf.RoundToInt(coord.z) / CELL_SIZE);
        pos -= new Vector2Int((int)_fieldStart.position.x, (int)_fieldStart.position.z);
        return pos;
    }

    public bool CanPlace(PieceData data, Vector2Int pos) {
        if (pos.x < 0 || pos.y < 0) {
            return false;
        }

        if (pos.x + data.Cells.GetLength(0) - 1 >= _field.GetLength(0)) {
            return false;
        }

        if (pos.y + data.Cells.GetLength(1) - 1 >= _field.GetLength(1)) {
            return false;
        }

        for (int x = 0; x < data.Cells.GetLength(0); x++) {
            for (int y = 0; y < data.Cells.GetLength(1); y++) {
                if (data.Cells[x, y] && _field[pos.x + x, pos.y + y] != null) {
                    return false;
                }
            }
        }

        return true;
    }

    public void PlacePiece(PieceData pieceData) {
        PlacePiece(pieceData, GetPieceClampedPosOnField());
        _nextBlocks.Remove(pieceData);
        _placedPiecesAmount++;
        if (MainGameConfig.resourceOnPlaceCell)
            CollectResourcesOnPlace(pieceData);
        ExplodeCells();
        //  GoalView.Instance.UpdateTask(GameData);

        if (CheckWin()) {
            // Win();
            return;
        }

        if (_placedPiecesAmount % 3 == 0) {
            Debug.Log("auto generate new pieces");
            GenerateNewPieces();
        }

        if (CheckLose()) {
            Lose();
        }
    }

    private void PlacePiece(PieceData pieceData, Vector2Int pos) {
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++) {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++) {
                if (!pieceData.Cells[x, y]) {
                    continue;
                }

                var place = new Vector2Int((int)Mathf.Clamp(pos.x + x, 0, MainGameConfig.fieldSize),
                    (int)Mathf.Clamp(pos.y + y, 0, MainGameConfig.fieldSize));
                // var go = Instantiate(PiecesViewTable.Instance.GetCellByType(pieceData.Type.cellType), _fieldContainer);
                var go = Instantiate(pieceData.Type.cellPrefab, _fieldContainer);
                go.transform.localPosition = new Vector3(place.x, -0.45f, place.y);
                _field[place.x, place.y] = pieceData.Type;
                _cells[place.x, place.y] = go;
                go.GetComponent<CellView>().PlaceCellOnField();
                SpawnResourceFx(pieceData, place, go);
            }
        }

        ShakeCamera();
        CheckPlacedCellsForTask();
    }

    private void SpawnResourceFx(PieceData pieceData, Vector2Int place, CellView go) {
        string needText = " +";
        var resourcesForPlace = _field[place.x, place.y].resourcesForPlace;
        for (int i = 0; i < resourcesForPlace.Length; i++)
            needText += resourcesForPlace[i].resourceCount + " <sprite name=" + resourcesForPlace[i].resourceType + ">" + " ";
        if (!_placedCellsCount.TryAdd(pieceData.Type.cellType, 1))
            _placedCellsCount[pieceData.Type.cellType]++;
        if (resourcesForPlace.Length != 0) {
            var canvasPosition = _raycastCamera.WorldToScreenPoint(go.transform.position);
            ShowFloatingText(needText, canvasPosition, 20, 1);
        }
    }

    private void CollectResourcesOnPlace(PieceData placedPiece) {
        for (int x = 0; x < placedPiece.Cells.GetLength(0); x++) {
            for (int y = 0; y < placedPiece.Cells.GetLength(1); y++) {
                if (!placedPiece.Cells[x, y]) {
                    continue;
                }

                var needResources = placedPiece.Type.resourcesForPlace;
                for (int i = 0; i < needResources.Length; i++) {
                    var resourceType = needResources[i];
                    if (resourceType == null) {
                        continue;
                    }

                    if (!GameData.CollectedResources.TryAdd(resourceType.resourceType, resourceType.resourceCount))
                        GameData.CollectedResources[resourceType.resourceType] += resourceType.resourceCount;
                }
            }
        }

        CheckResourceCountForTasks();
    }

    private void CheckResourceCountForTasks()
    {
        for (int i = 0; i < _currentTasks.Count; i++)
        {
            if (_currentTasks[i].taskInfo.taskType == TaskInfo.TaskType.getResource)
            {
                if (_currentTasks[i].taskInfo.needResource == ResourceType.None &&
                    GameData.CollectedResources.Count != 0)
                {
                    ResourceType maxResourceType = ResourceType.None;
                    foreach (var resource in GameData.CollectedResources)
                    {
                        if (maxResourceType == ResourceType.None ||
                            GameData.CollectedResources[maxResourceType] < resource.Value)
                            maxResourceType = resource.Key;
                    }

                //    if (GameData.CollectedResources[maxResourceType] !=
                  //      (int)_currentTasks[i].taskUIView.filledBarImage.value)
                 //   {
                       // _currentTasks[i].taskUIView.AddTextAnimation(GameData.CollectedResources[maxResourceType]);
                      //  _currentTasks[i].taskUIView.currentTaskValue.text =
                        //    GameData.CollectedResources[maxResourceType] + " / " + _currentTasks[i].taskInfo.count +
                        //    " <sprite name=" + maxResourceType + ">";
                        //   _currentTasks[i].taskUIView.filledBarImage.value = GameData.CollectedResources[maxResourceType];
                        if (_currentTasks[i].taskInfo.count <= GameData.CollectedResources[maxResourceType])
                        {
                            _currentTasks[i].taskUIView.CompleteTask();
                            _currentTasks.RemoveAt(i);
                            break;
                            // Debug.Log(resource.Key);
                        }
                   // }
                }
                else if (GameData.CollectedResources.TryGetValue(_currentTasks[i].taskInfo.needResource,
                             out int resourceCount))
                {
                   // if ((int)_currentTasks[i].taskUIView.filledBarImage.value != resourceCount)
                  //  {
                       // _currentTasks[i].taskUIView.AddTextAnimation(resourceCount);
                     //   _currentTasks[i].taskUIView.currentTaskValue.text =
                      //      resourceCount + " / " + _currentTasks[i].taskInfo.count;
                       // _currentTasks[i].taskUIView.filledBarImage.value = resourceCount;
                       if (resourceCount >= _currentTasks[i].taskInfo.count)
                       {
                           _currentTasks[i].taskUIView.CompleteTask();
                            _currentTasks.RemoveAt(i);
                       }
                  // }
                }
            }
        }
    }

    private void CheckPlacedCellsForTask() {
        for (int i = 0; i < _currentTasks.Count; i++) {
            if (_currentTasks[i].taskInfo.taskType == TaskInfo.TaskType.placeNeedCell &&
                _placedCellsCount.TryGetValue(_currentTasks[i].taskInfo.needCell.cellType, out int count)) {
                //if ((int)_currentTasks[i].taskUIView.filledBarImage.value != count)
               // {
               // _currentTasks[i].taskUIView.AddTextAnimation(count);
              //  _currentTasks[i].taskUIView.currentTaskValue.text = count + " / " + _currentTasks[i].taskInfo.count;
               // _currentTasks[i].taskUIView.filledBarImage.value = count;
                if (_currentTasks[i].taskInfo.count <= count)
                {
                    _currentTasks[i].taskUIView.CompleteTask();
                    _currentTasks.RemoveAt(i);
                }
            }
        }
    }

    private void CheckMonoLinesForTasks()
    {
        for (int i = 0; i < _currentTasks.Count; i++)
        {
            if (_currentTasks[i].taskInfo.taskType == TaskInfo.TaskType.placeMonoLine &&
                _monoLinesCount.TryGetValue(_currentTasks[i].taskInfo.needResource, out int count))
            {
               // if (_currentTasks[i].taskInfo.needResource == count)
               // {
                    //_currentTasks[i].taskUIView.AddTextAnimation();
                    //_currentTasks[i].taskUIView.currentTaskValue.text = count + " / " + _currentTasks[i].taskInfo.count;
                    if (_currentTasks[i].taskInfo.count <= count)
                    {
                        _currentTasks[i].taskUIView.CompleteTask();
                        _currentTasks.RemoveAt(i);
                        i--;
                    }
                //}
            }
        }
    }

    private void CheckUnlockedCellForTask(CellTypeInfo needCell) {
        for (int i = 0; i < _currentTasks.Count; i++) {
            if (_currentTasks[i].taskInfo.taskType == TaskInfo.TaskType.unlockCell &&
                _currentTasks[i].taskInfo.needCell == needCell)
            {
                //_currentTasks[i].taskUIView.currentTaskValue.text = "1/1";
           // _currentTasks[i].taskUIView.AddTextAnimation(1);
            _currentTasks[i].taskUIView.CompleteTask();
                _currentTasks.RemoveAt(i);
            }
        }
    }

    private void ExplodeCells() {
        int width = _field.GetLength(0);
        int height = _field.GetLength(1);
        string unlockedCellText = "";
        // Проверка строк
        for (int y = 0; y < height; y++) {
            bool fullRow = true;

            for (int x = 0; x < width; x++) {
                if (_field[x, y] == null) {
                    fullRow = false;
                    break;
                }
            }

            if (fullRow) {
                DestroyLine(y, width, true, ref unlockedCellText);
            }
        }

        // Проверка столбцов
        for (int x = 0; x < width; x++) {
            bool fullColumn = true;

            for (int y = 0; y < height; y++) {
                if (_field[x, y] == null) {
                    fullColumn = false;
                    break;
                }
            }

            if (fullColumn) {
                DestroyLine(x, height, false, ref unlockedCellText);
            }
        }
        if (unlockedCellText != "")
            ShowFloatingText(unlockedCellText +" is unlocked!", _floatingTextContainer.position, 40,2.5f);
        DestroyAllMarkedCells();
    }

    private void DestroyAllMarkedCells()
    {
        for (int i = 0; i < _cellsToDestroy.Count; i++)
        {
            var cell = _cellsToDestroy[i];
            _cellsToDestroy.RemoveAt(i--);
            DestroyCell((int)cell.x, (int)cell.y);
        }
    }
    private void DestroyLine(int mainAxisCurrentValue, int secondAxisLenght, bool isRow, ref string unlockedCellText) //cut this to pieces
    {
        bool fullSameResourcesColumn = MainGameConfig.bonusResourcesOnDestroyLine ? true : false;
        int bonusResourcesOnDestroyLine = 0;
        ResourceType currentBonusResourceType = ResourceType.None;
        Dictionary<ResourceType, int> resourcesMultiplayers = new Dictionary<ResourceType, int>();
        Dictionary<CellTypeInfo, int> cellTypesInLine = new Dictionary<CellTypeInfo, int>();

        CellType currentCellType = CellType.Empty;
        fullSameResourcesColumn = true;
        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++)
        {
            Vector2 curPosition = !isRow
                ? new Vector2(mainAxisCurrentValue, secondAxis)
                : new Vector2(secondAxis, mainAxisCurrentValue);
            var cellInfo = _field[(int)curPosition.x, (int)curPosition.y];
            if (fullSameResourcesColumn)
            {
                if (currentCellType == CellType.Empty)
                    currentCellType = cellInfo.cellType;
                else if (currentCellType != cellInfo.cellType)
                    fullSameResourcesColumn = false;
            }

            if (cellInfo.MultiplayerForSameResourceType != 0 &&
                !resourcesMultiplayers.TryAdd(cellInfo.resourcesForDestroy[0].resourceType,
                    cellInfo.MultiplayerForSameResourceType))
            {
                if (cellInfo.MultiplayerForSameResourceType <
                    resourcesMultiplayers[cellInfo.resourcesForDestroy[0].resourceType])
                    resourcesMultiplayers[cellInfo.resourcesForDestroy[0].resourceType] =
                        cellInfo.MultiplayerForSameResourceType;
            }
        }

        for (int secondAxis = 0; secondAxis < secondAxisLenght; secondAxis++)
        {
            Vector2 curPosition = !isRow
                ? new Vector2(mainAxisCurrentValue, secondAxis)
                : new Vector2(secondAxis, mainAxisCurrentValue);
            var cellInfo = _field[(int)curPosition.x, (int)curPosition.y];

            string floatingText = "+ ";

            if (!cellTypesInLine.TryAdd(cellInfo, 1))
                cellTypesInLine[cellInfo]++;

            for (int i = 0; i < cellInfo.resourcesForDestroy.Length; i++)
            {
                resourcesMultiplayers.TryGetValue(cellInfo.resourcesForDestroy[i].resourceType,
                    out int resourceMultiplayer);
                Debug.Log(resourceMultiplayer + " resource multiplayer" + cellInfo.resourcesForDestroy[i].resourceType);
                if (resourceMultiplayer == 0)
                    resourceMultiplayer = 1;
                int count = cellInfo.resourcesForDestroy[i].resourceCount * resourceMultiplayer;
                if (!GameData.CollectedResources.TryAdd(cellInfo.resourcesForDestroy[i].resourceType,
                        count))
                    GameData.CollectedResources[cellInfo.resourcesForDestroy[i].resourceType] += count;
                floatingText +=  " <sprite name=" + cellInfo.resourcesForDestroy[i].resourceType + "> " + count +
                                " ";
                if (fullSameResourcesColumn)
                {
                    bonusResourcesOnDestroyLine +=
                        cellInfo.resourcesForDestroy[i]
                            .resourceCount; //fix this if on destroy resources types be more than 1;
                    currentBonusResourceType = cellInfo.resourcesForDestroy[i].resourceType;
                }
            }

            if (cellInfo.resourcesForDestroy.Length != 0)
            {
                var canvasPosition =
                    _raycastCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform
                        .position);
                ShowFloatingText(floatingText, canvasPosition, 20, 1);
            }

            // CollectResources( _field[(int)curPosition.x, (int)curPosition.y]);
            _cellsToDestroy.Add(new Vector2(curPosition.x, curPosition.y));
        }

        if (fullSameResourcesColumn && currentBonusResourceType != ResourceType.None /* &&
            !GameData.CollectedResources.TryAdd(currentBonusResourceType, bonusResourcesOnDestroyLine)*/)
        {
            if (!_monoLinesCount.TryAdd(currentBonusResourceType, 1))
                _monoLinesCount[currentBonusResourceType]++;
            //Debug.Log("mono line "+ currentBonusResourceType + " "+_monoLinesCount[currentBonusResourceType]);
            CheckMonoLinesForTasks();
            GameData.CollectedResources[currentBonusResourceType] += bonusResourcesOnDestroyLine;
            Vector2 curPosition = !isRow ? new Vector2(mainAxisCurrentValue, 5) : new Vector2(5, mainAxisCurrentValue);
            var needPosition =
                _raycastCamera.WorldToScreenPoint(_cells[(int)curPosition.x, (int)curPosition.y].transform.position);

            ShowFloatingText("<sprite name=" + currentBonusResourceType + "> " + bonusResourcesOnDestroyLine, needPosition, 30,
                1.5f);
        }
        else
            Debug.Log("not full same");

        for (int i = 0; i < _currentCraftedCells.Count; i++)
        {
            bool addNewCell = false;
            for (int j = 0; j < _currentCraftedCells[i].cellTypeToCraft.Length; j++)
            {
                if (cellTypesInLine.ContainsKey(_currentCraftedCells[i].cellTypeToCraft[j]))
                {
                    for (int x = 0; x < _currentCraftedCells[i].cellTypeToCraftSecond.Length; x++)
                    {
                        if (cellTypesInLine.ContainsKey(_currentCraftedCells[i].cellTypeToCraftSecond[x]))
                        {
                            currentCellsToSpawn.Add(_currentCraftedCells[i].cellsToCraft);
                            CheckUnlockedCellForTask(_currentCraftedCells[i].cellsToCraft);
                            unlockedCellText += _currentCraftedCells[i].cellsToCraft.cellName + "\n";

                            _currentCraftedCells.RemoveAt(i);
                            i--;
                            addNewCell = true;
                            CalculateCellSpawnChances();
                            break;
                        }
                    }

                    if (addNewCell) break;
                }
            }

            Debug.Log(addNewCell + " add new cell" + _currentCraftedCells.Count);
        }

        CheckResourceCountForTasks();

        //if(_currentTasks.Count == 0)
        //   Win();
    }

    private void DestroyCell(int x, int y) {
        _field[x, y] = null;
        _cells[x, y].DestroyCell();
      //  Destroy(_cells[x, y].gameObject);
    }

    private bool CheckWin() {
        /*var goal = GameData.TaskData.GoalToCollect.First();
        if (GameData.CollectedResources.TryGetValue(goal.Key, out int hasValue))
        {
            if (hasValue >= goal.Value)
            {
                return true;
            }
        }*/
        if (_currentTasks.Count == 0) {
            Win();
           // _buttonEndGameText.text = "Next level";
            return true;
        }

        return false;
    }

    private bool CheckLose() {
        foreach (PieceData t in _nextBlocks) {
            if (PieceUtils.CanPlacePiece(_field, _nextBlocks[0].Cells)) {
                return false;
            }
        }
        //_buttonEndGameText.text = "Restart";
        return true;
    }

    private void Win() {
        StorageManager.gameDataMain.CurMaxLevel++;
        StorageManager.SaveGame();
        Debug.Log(" win");
        _mainTextUp.text = "You win!";
        foreach (var taskUI in _taskUIViews)
            taskUI.gameObject.SetActive(false);

        GoalView.Instance.SetWinState();
    }

    private void Lose() {
        _mainTextUp.text = "You lose:(";
        foreach (var taskUI in _taskUIViews)
            taskUI.gameObject.SetActive(false);
        GoalView.Instance.SetLoseState();
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    private void CalculateCellSpawnChances()
    {
        float lastChance = 0;
        CellsChanceToSpawn = new float[currentCellsToSpawn.Count];
       Debug.Log(CellsChanceToSpawn.Length + " ch list count"); 
        for (int i = 0; i < currentCellsToSpawn.Count; i++)
        {
            lastChance += currentCellsToSpawn[i].ChanceToSpawn;
            CellsChanceToSpawn[i] = lastChance;
        }
    }
    public void Reset() {
        GenerateField();
        GenerateTask();
        StartGame();
       // Debug.Log(StorageManager.gameDataMain + " cur level");
        if (StorageManager.gameDataMain.CurMaxLevel < 20)
            _currentLevelConfig = MainGameConfig.levels[StorageManager.gameDataMain.CurMaxLevel];
        else {
            Debug.Log("meta");
        }

        // _nextBlocks = new List<PieceData>();

        _placedPiecesAmount = 0;
        // mainGameConfig.fieldSize = 10;
        _field = new CellTypeInfo[MainGameConfig.fieldSize, MainGameConfig.fieldSize];
        _cells = new CellView[MainGameConfig.fieldSize, MainGameConfig.fieldSize];

        _currentCraftedCells = new List<CraftingCellInfo>();
        foreach (var craftedCell in MainGameConfig.cellsToCraft)
            _currentCraftedCells.Add(craftedCell);

        var startCells = _currentLevelConfig.CellTypesTableConfig;
        currentCellsToSpawn = new List<CellTypeInfo>();
        for (int i = 0; i < startCells.cellsToSpawn.Length; i++)
            currentCellsToSpawn.Add(startCells.cellsToSpawn[i]);
        CalculateCellSpawnChances();
        Debug.Log(currentCellsToSpawn.Count + " cells to spawn");
        _placedCellsCount = new Dictionary<CellType, int>();

        _currentTasks = new List<TaskInfoAndUI>();
        foreach (var uiTaskView in _taskUIViews) {
            uiTaskView.gameObject.SetActive(false);
        }

        for (int i = 0; i < _currentLevelConfig.Tasks.Length; i++) {
            var task = _currentLevelConfig.Tasks[i];
            var taskUI = _taskUIViews[i];
            taskUI.gameObject.SetActive(true);
            _currentTasks.Add(new TaskInfoAndUI(task, taskUI));
            string needTasktext = "";
            switch (task.taskType) {
                case TaskInfo.TaskType.getResource:

                    if (task.needResource == ResourceType.None)
                        needTasktext = " Get " + task.count + " of any resource";
                    else
                        needTasktext = " Get " + task.count + " <sprite name=" + task.needResource + ">";
                    break;

                case TaskInfo.TaskType.placeMonoLine:

                    needTasktext = " Place mono line " + task.count + " times with " + " <sprite name=" + task.needResource + ">";
                    break;

                case TaskInfo.TaskType.placeNeedCell:

                    needTasktext = " Place " + task.needCell.cellName + " " + task.count + " times";
                    break;

                case TaskInfo.TaskType.unlockCell:

                    needTasktext = " Unlock " + task.needCell.cellName;
                    break;
            }

            StartCoroutine(taskUI.taskInfoTextHelper.StartSpawnText(needTasktext));

            // taskUI.currentTaskValue.text = "0 / " + task.count;
            // taskUI.filledBarImage.value = 0;
            // taskUI.filledBarImage.maxValue = task.count;
            //show task info in texts
        }

        //BgTasksImage.sizeDelta = new Vector2(BgTasksImage.sizeDelta.x,BgTasksImage.sizeDelta.y * _currentTasks.Count/3);
        _monoLinesCount = new Dictionary<ResourceType, int>();

        StartCoroutine(_characterInfoTextHelper.StartSpawnText(_currentLevelConfig.GuideForLevelText));

        currentGuaranteedFirstCells = new List<CellTypeInfo>();
        foreach (var cellInfo in _currentLevelConfig.CurrentGuaranteedFirstCells)
            currentGuaranteedFirstCells.Add(cellInfo);

        GameData = new GameData();

        GenerateNewPieces();
    }

    public void ShowFloatingText(string needText, Vector2 newPosition, float textSize, float showTime) {
        //  var screenPoint = (Input.mousePosition - _mainCanvasRectTransform.position) / _mainCanvasRectTransform.localScale.x;
        var floatingText = _floatingTextsPool.Get();
        floatingText.SetText(newPosition, needText, textSize, showTime);
    }

    public void ReleaseFloatingText(FloatingTextView needTextObject) {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }

    public void GoToMeta() {
        SceneManager.LoadScene("MetaScene");
    }
}

[SerializeField]
public class PieceData {
    public bool[,] Cells;
    public CellTypeInfo Type;
}

[SerializeField]
public enum CellType {
    Empty = 0,
    Forest,
    Metal,
    Village,
    Mountain,
    MiniCity,
    FoodSource
}