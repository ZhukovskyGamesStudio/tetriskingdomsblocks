using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class MetaManager : BaseManager {
    //protected HeroType[,] _heroesOnField;
    public static MetaManager Instance { get; private set; }
    private List<ResourceMarkAndPieces> _connectedGroups = new List<ResourceMarkAndPieces>();
    [SerializeField] private Transform _cellContainer;
    [field:SerializeField]
    public MainMetaConfig MainMetaConfig { get;private set; }
    private PieceData _nextPiece = null;
    [SerializeField]private TMP_Text[] _resourcesCountText;
    [SerializeField]private TMP_Text _getPieceTimerText;
    [SerializeField]private TMP_Text _destroyPieceText;
    private int[,] _groupCellIndex;
    private int _minutesToGetPiece = 120;
    private bool _isDestroyPieceMode;
    [SerializeField] private Transform _hummerContainer;
    [SerializeField] private Transform _hummerContainerStart;
    [SerializeField] private Transform _hummerContainerEnd;
    private Tween _hummerTween;
    [SerializeField] private Transform _resourcesMarksContainer;
    [SerializeField] private LayerMask _pieceMask;
    [SerializeField] private ResourceMarkView resourceMarkViewPrefab;
    private Sequence hummerSequence;
 //   private bool _isDraggingCamera;
 //   public bool IsDraggingPiece;
  //  private Vector3 _dragStartPosition;
    [HideInInspector]
    public Vector3 ScreenToWorldPointOnPiece => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition),
        out RaycastHit hit, Mathf.Infinity, _pieceMask)
        ? hit.point
        : Vector3.zero;

    [HideInInspector]
    public Vector3 TouchToWorldPointOnPiece => Physics.Raycast(_mainCamera.ScreenPointToRay(Input.GetTouch(0).position),
        out RaycastHit hit, Mathf.Infinity, _pieceMask)
        ? hit.point
        : Vector3.zero;
    
    private ObjectPool<ResourceMarkView> _resourcesMarksPool;
    protected override void Awake() {
        base.Awake();    
        Instance = this;
        _resourcesMarksPool =
            new ObjectPool<ResourceMarkView>(() => Instantiate(resourceMarkViewPrefab,_resourcesMarksContainer));
    }

    protected override void Update()
    {
        base.Update();
        if (_hasInternetConnection &&
            (_currentGameTime - StorageManager.GameDataMain.LastGetPieceTimeDateTime).TotalHours < 2)
        {
            _getPieceTimerText.text = TimeConverter.ConvertToTimeString(GetTimeUntilNextPiece())+" to \n new piece";
        }

        if (Input.GetMouseButtonDown(0) )
        {
            if(_isDestroyPieceMode)
            TryDestroyPiece();
           // _isDraggingCamera = true;
           // _dragStartPosition = Input.mousePosition;
        }
     /*   else if(Input.GetMouseButtonUp(0))
            _isDraggingCamera = false;*/
        
     /*   if(_isDraggingCamera && !IsDraggingPiece)
            DragCamera();*/
    }

   /* private void DragCamera()
    {
           Vector3 pos = _mainCamera.ScreenToViewportPoint(Input.mousePosition - _dragStartPosition);
       Vector3 move = new Vector3(pos.x * MainMetaConfig.CameraDragSpeed, 0, pos.y * MainMetaConfig.CameraDragSpeed);
            
       var needPosition =CameraContainer.transform.position-move;
        
        CameraContainer.position =  new Vector3(Mathf.Clamp(needPosition.x,_fieldStart.position.x,_fieldEnd.position.x),
            needPosition.y, 
            Mathf.Clamp(needPosition.z,_fieldStart.position.z,_fieldEnd.position.z));
       _dragStartPosition = Input.mousePosition;
    }*/
    private void TryDestroyPiece()
    {
        Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition),
            out RaycastHit hit, Mathf.Infinity, _pieceMask);
        if (hit.collider != null && (StorageManager.GameDataMain.resourcesCount[0] >= 500 &&
                                     StorageManager.GameDataMain.resourcesCount[1] >= 500 &&
                                     StorageManager.GameDataMain.resourcesCount[2] >= 500))
        {
            Vector3 cellPos = new Vector3(Mathf.RoundToInt(hit.collider.transform.localPosition.x),Mathf.RoundToInt(hit.collider.transform.localPosition.y),Mathf.RoundToInt(hit.collider.transform.localPosition.z)) ;
           // if(_groupCellIndex[Mathf.RoundToInt(cellPos.x), Mathf.RoundToInt(cellPos.z)] == 0)return;
            StorageManager.GameDataMain.resourcesCount[0] -= 500;
            StorageManager.GameDataMain.resourcesCount[1] -= 500;
            StorageManager.GameDataMain.resourcesCount[2] -= 500;
            
            int groupIndex = _groupCellIndex[(int)cellPos.x, (int)cellPos.z];
            _groupCellIndex[(int)cellPos.x, (int)cellPos.z] = 0;
                CollectResourcesFromMark(groupIndex-1,1);
               _connectedGroups[groupIndex-1].ResourceMarkView.CollectAnimation();
               // _connectedGroups[groupIndex-1].ResourceMarkView.gameObject.SetActive(false);
                HummerDestoyPieceAnimation(_cells[(int)cellPos.x, (int)cellPos.z]);
            //_cells[(int)cellPos.x, (int)cellPos.z].DestroyCell();
            _field[(int)cellPos.x, (int)cellPos.z] = CellType.Empty;
            StorageManager.GameDataMain.FieldRows[(int)cellPos.x].RowCells[(int)cellPos.z] = new ResourceAndCountData(_field[(int)cellPos.x, (int)cellPos.z],0);
            
                RecalculateCellGroupAfterDeletePiece(groupIndex);
                
            //reset resource marks
        }
    }

    public void RecalculateCellGroupAfterDeletePiece(int groupIndex)
    {
        if (_connectedGroups[groupIndex - 1].Pieces.Count == 1)
        {
            ReleaseResourceMark(_connectedGroups[groupIndex - 1].ResourceMarkView);
            _connectedGroups[groupIndex - 1] = new ResourceMarkAndPieces(null, new List<(int, int)>());
            return;
        }

        var checkedCells = new int[_field.GetLength(0), _field.GetLength(1)];
        int curMaxIndex = 1;
        foreach (var (row, col) in _connectedGroups[groupIndex - 1].Pieces)
        {
            if (checkedCells[row, col] != 0 || _field[row, col] == CellType.Empty) continue;

            int curGroupIndex = 0;
            var checkedCellType = _field[row, col];
            foreach (var dir in FieldUtils.Directions) {
                Vector2Int combined = dir + new Vector2Int(col, row);
                if (combined.y >= _field.GetLength(0) || combined.x >= _field.GetLength(1) || combined.y < 0 ||
                    combined.x < 0) continue;

                if (checkedCellType == _field[combined.y, combined.x])
                {
                    if (checkedCells[combined.y, combined.x] != 0)
                    {
                        if (curGroupIndex == 0)
                            curGroupIndex = checkedCells[combined.y, combined.x];
                        else
                        {
                            int newIndex = checkedCells[combined.y, combined.x];
                            foreach (var (cellRow, cellCol) in _connectedGroups[groupIndex - 1].Pieces)
                            {
                                if (checkedCells[cellRow, cellCol] == newIndex)
                                    checkedCells[cellRow, cellCol] = curGroupIndex;
                            }
                        }
                    }
                    else
                        checkedCells[combined.y, combined.x] = curGroupIndex;
                }
            }

            if (curGroupIndex == 0)
                curGroupIndex = curMaxIndex++;

            checkedCells[row, col] = curGroupIndex;
        }

        Dictionary<int, List<(int row, int col)>> cellsGroupIndex = new Dictionary<int, List<(int row, int col)>>();
        for (int i = 0; i < checkedCells.GetLength(0); i++)
        {
            for (int j = 0; j < checkedCells.GetLength(1); j++)
            {
                if (checkedCells[i, j] == 0) continue;
                if (cellsGroupIndex.ContainsKey(checkedCells[i, j]))
                    cellsGroupIndex[checkedCells[i, j]].Add((i, j));
                else
                    cellsGroupIndex.Add(checkedCells[i, j], new List<(int row, int col)> { (i, j) });
            }
        }

        ReleaseResourceMark(_connectedGroups[groupIndex - 1].ResourceMarkView);
        
        _connectedGroups[groupIndex - 1] = new ResourceMarkAndPieces();
        List<int> emptyIndexes = new List<int>();
        for (int i = 0; i < _connectedGroups.Count; i++)
        {
            if (_connectedGroups[i].ResourceMarkView == null)
                emptyIndexes.Add(i);
        }

        Color needColor = Color.clear;
        foreach (var checkedCell in cellsGroupIndex)
        {
            Vector3 collectResourceMarkPosition = new Vector3();
            foreach (var (row, col) in checkedCell.Value)
            {
                collectResourceMarkPosition += _cells[row, col].transform.position;

                if (needColor == Color.clear)
                {
                    needColor = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                        (c => c.CellType == _field[row, col]).MarkCellColor;
                    needColor.a = 1;
                }
            }

            var cellMarkView =
                SpawnResourceMark(collectResourceMarkPosition / checkedCell.Value.Count, 0, 0, ResourceType.None,
                    needColor);
            cellMarkView.gameObject.SetActive(false);
            var resourceMarkAndPieces = new ResourceMarkAndPieces(cellMarkView, checkedCell.Value);

            int needIndex = 0;
            if (emptyIndexes.Count > 0)
            {
                _connectedGroups[emptyIndexes[0]] = resourceMarkAndPieces;
                needIndex = emptyIndexes[0] + 1;
                emptyIndexes.RemoveAt(0);
            }
            else
            {
                _connectedGroups.Add(resourceMarkAndPieces);
                needIndex = _connectedGroups.Count;
            }

            foreach (var (row, col) in checkedCell.Value)
                _groupCellIndex[row, col] = needIndex;
        }
    }

    private ResourceMarkView SpawnResourceMark(Vector3 pos,int maxResource, int currentResource, ResourceType resourceType, Color resourceColor)
    {
        var mark = _resourcesMarksPool.Get();
        mark.gameObject.SetActive(true);
        //pos = _mainCamera.WorldToScreenPoint(pos);
        mark.transform.position = new Vector3(pos.x,pos.y+1,pos.z);
        mark.SetColor(resourceColor);
        mark.SetResourceMarkInfo(maxResource,currentResource,resourceType,_connectedGroups.Count);
        return mark;
    }

    private void ReleaseResourceMark(ResourceMarkView mark)
    {
        //mark.gameObject.SetActive(false);
        _resourcesMarksPool.Release(mark);
    }
    public TimeSpan GetTimeUntilNextPiece()
    {
       // if (StorageManager.GameDataMain.HealthCount >= MAX_HEALTH_COUNT) return TimeSpan.Zero;
        
        TimeSpan timeSinceLastUpdate = _currentGameTime - StorageManager.GameDataMain.LastGetPieceTimeDateTime;
        double minutesPassed = timeSinceLastUpdate.TotalMinutes;
        double minutesUntilNext = _minutesToGetPiece - (minutesPassed % _minutesToGetPiece);
        
        return TimeSpan.FromMinutes(minutesUntilNext);
    }
    public void Play() {
        if (StorageManager.GameDataMain.HealthCount != 0)
        {
            StorageManager.GameDataMain.LastExitTime =_currentGameTime.ToString(CultureInfo.InvariantCulture);
                StorageManager.SaveGame();
        SceneManager.LoadScene("GameScene");
        }
        else
        {
            
            //floating window with "watch ad and get health"
        }
    }

    public void BuyPiece() {
       if (StorageManager.GameDataMain.resourcesCount[0] >= 100 &&
           StorageManager.GameDataMain.resourcesCount[1] >= 100 && StorageManager.GameDataMain.resourcesCount[2] >= 100
           && _nextPiece == null)
       {
       // DialogsManager.Instance.ShowDialog(typeof(BuyPieceDialog));
       StorageManager.GameDataMain.resourcesCount[0] -= 100;
       StorageManager.GameDataMain.resourcesCount[1] -= 100;
       StorageManager.GameDataMain.resourcesCount[2] -= 100;
       UpdateResourcesCountUIText();
       GenerateNewPieces(); // for test
       
       }
    }

    public void ToggleDestroyPieceMode()
    {
        _isDestroyPieceMode = !_isDestroyPieceMode;
        _hummerTween.Kill();
       
          //  .Join(CameraContainer.transform.DOMoveY(camPos.y * Random.Range(1.01f, 1.03f), 0.2f))
            //.Join(CameraContainer.transform.DOMoveZ(camPos.z - zOffset, 0.1f))
           // .Append(CameraContainer.transform.DOMoveX(camPos.x + xOffset, 0.1f))
           // .Join(CameraContainer.transform.DOMoveZ(camPos.z + zOffset, 0.1f)).Append(CameraContainer.transform.DOMove(camPos, 0.1f));
        if (_isDestroyPieceMode)
        {
            _destroyPieceText.text = "Cancel"; 
            
            hummerSequence.Kill();
            hummerSequence = DOTween.Sequence();

            hummerSequence.Append(_hummerContainer.transform.DOMove(_hummerContainerStart.position, 0.8f));
            
            Tween floatTween = _hummerContainer.DOMoveY(_hummerContainer.position.y+1, 0.5f)
                .SetLoops(1000, LoopType.Yoyo);
            
            hummerSequence.Append(floatTween);
        }
        else
        {
            hummerSequence.Kill();
            _destroyPieceText.text = "Destroy pieces mode";
            hummerSequence
                .Append(_hummerContainer.transform.DOMove(_hummerContainerEnd.position, 0.8f));
        }
    }

    private void HummerDestoyPieceAnimation(CellView cell)
    {
        DestroyPieceWithHummer(cell).Forget();
        cell.OffCollider();
        hummerSequence.Kill();
        hummerSequence = DOTween.Sequence();

        hummerSequence.Append(_hummerContainer.transform.DOMove(new Vector3(cell.transform.position.x + 1,cell.transform.position.y,cell.transform.position.z) , 0.8f))
            .Append(_hummerContainer.transform.DORotate(new Vector3(0, 0, 90f), 0.2f))
        .Append(_hummerContainer.transform.DORotate(new Vector3(0,0,0f), 0.2f))
            .Append(_hummerContainer.transform.DOMove(_hummerContainerStart.position, 0.8f));
        
        Tween floatTween = _hummerContainer.DOMoveY(_hummerContainer.position.y+1, 0.5f)
            .SetLoops(-1, LoopType.Yoyo);
        hummerSequence.Append(floatTween);
    }
    private async UniTask DestroyPieceWithHummer(CellView cell)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        cell.DestroyCell();
        VibrationsManager.Instance.SpawnVibration(VibrationType.PlacePiece);
        ShakeCamera();
    }
    public void UpdateResourcesCountUIText()
    {
        for (int i = 0; i < _resourcesCountText.Length; i++)
            _resourcesCountText[i].text = StorageManager.GameDataMain.resourcesCount[i].ToString();
    }

    public void GetPiece()
    {
        if (_hasInternetConnection && _nextPiece == null &&
            (_currentGameTime - StorageManager.GameDataMain.LastGetPieceTimeDateTime).TotalHours >= 2)
        {
            StorageManager.GameDataMain.LastGetPieceTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
            GenerateNewPieces(); // for test
        }
    }

    public void CollectAll() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }
    
    public void GenerateNewPieces()
    {
        _nextPiece = PieceUtils.GetNewPiece(guaranteed: null);
        NextPiecesView.Instance.SetData(_nextPiece);
    }

    protected override void SetupGame()
    {
        _field = new CellType[MainMetaConfig.FieldSize, MainMetaConfig.FieldSize];
        _cells = new CellView[MainMetaConfig.FieldSize, MainMetaConfig.FieldSize];
        CalculateFiguresSpawnChances();
        var startCells = MainMetaConfig.CellsConfigs;
        _currentCellsToSpawn = new List<CellTypeInfo>();
        for (int i = 0; i < startCells.Count; i++)
            _currentCellsToSpawn.Add(startCells[i]);
        CalculateCellSpawnChances();
      //  Debug.Log(StorageManager.GameDataMain.FieldRows +" "+  (StorageManager.GameDataMain.FieldRows.Length > 1));
        if(!StorageManager.GameDataMain.FieldSaveIsCreated)
        { 
                     StorageManager.GameDataMain.LastGetPieceTime = (_currentGameTime - TimeSpan.FromHours(2)).ToString(CultureInfo.InvariantCulture); 
                    StorageManager.GameDataMain.LastExitTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
                    
            StorageManager.GameDataMain.FieldSaveIsCreated = true;
            StorageManager.GameDataMain.FieldRows = new MetaFieldData[_field.GetLength(0)];
            for (int i = 0; i < _field.GetLength(0); i++)
            {
                StorageManager.GameDataMain.FieldRows[i].RowCells = new ResourceAndCountData[_field.GetLength(1)];
                for (int j = 0; j < _field.GetLength(1); j++)
                    StorageManager.GameDataMain.FieldRows[i].RowCells[j] = new ResourceAndCountData(_field[i, j], 0);
            }
        }
        else if (StorageManager.GameDataMain.FieldRows != null && StorageManager.GameDataMain.FieldRows.Length > 1)
        {
            _field = new CellType[StorageManager.GameDataMain.FieldRows.Length,StorageManager.GameDataMain.FieldRows[0].RowCells.Length];
            for (int i = 0; i < _field.GetLength(0); i++)
            {
                for (int j = 0; j < _field.GetLength(1); j++)
                {
                    _field[i,j] = StorageManager.GameDataMain.FieldRows[i].RowCells[j].CellType;
                    var cellType = _field[i, j];
                    if (cellType != CellType.Empty)
                    {
                        var prefab = PiecesViewTable.Instance.CellsViewList.GetCellByType(cellType);
                        var go = Instantiate(prefab, _fieldContainer);
                        go.transform.localPosition = new Vector3(i, -0.45f, j);
                        _cells[i, j] = go;

                       go.SetSeed(Guid.NewGuid());
                    }
                }
            }
        }
       
    //    Debug.Log(StorageManager.GameDataMain.FieldRows[0].RowCells.Length + " field size "+ StorageManager.GameDataMain.FieldRows.Length);
        UpdateResourcesCountUIText();
       
        GetResourceCollectMarks();
        
       InvokeRepeating("UpdateResourceMarks",MainMetaConfig.resourceMarksUpdateCouldown,MainMetaConfig.resourceMarksUpdateCouldown);
       
        base.SetupGame();
    }

    public void CollectResourcesFromMark(int index, float multiplayerResources)
    {
        Debug.Log("collect resource from"+index);
        int collectedResouces = 0;
        ResourceType curResource = ResourceType.None;
        foreach (var (row, col) in _connectedGroups[index].Pieces)
        {
            var cellConfig = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                (c => c.CellType == _field[row, col]);
            if (curResource == ResourceType.None)
                curResource = cellConfig.AfkResourceType;
            if (cellConfig.AfkResourceType != ResourceType.None)
            {
                collectedResouces += StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount;
                StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount = 0;
            }
        }
        StorageManager.GameDataMain.LastExitTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
        StorageManager.GameDataMain.resourcesCount[(int)curResource - 1] += (int)(collectedResouces * multiplayerResources);
        UpdateResourcesCountUIText();
    }

    protected override void SaveEnergyData() {
        StorageManager.GameDataMain.LastExitTime = _currentGameTime.ToString(CultureInfo.InvariantCulture);
        base.SaveEnergyData();
    }
    public void ShowCollectAllDialog() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }
    public void CollectResourcesFromAllMarks(float multiplayer)
    {
        foreach (var resourceMarkGroup in _connectedGroups)
        {
            resourceMarkGroup.ResourceMarkView.CollectAnimation();
            CollectResourcesFromMark(resourceMarkGroup.ResourceMarkView.markIndex, multiplayer);
        }
    }
  
    private void CalculateCellSpawnChances()
    {
        float lastChance = 0;
        CellsChanceToSpawn = new float[_currentCellsToSpawn.Count];
        for (int i = 0; i < _currentCellsToSpawn.Count; i++)
        {
            lastChance += _currentCellsToSpawn[i].ChanceToSpawn;
            CellsChanceToSpawn[i] = lastChance;
        }
    }

    public override void PlacePiece(PieceData pieceData, Vector2Int coord, CellView[,] cells, Transform cellsContainer) {
        base.PlacePiece(pieceData, coord, cells, cellsContainer);
        List<(int, int)> placedCells = GetPlacedCells(pieceData, coord);

        UpdateResourceMarksAfterPlacePiece(placedCells);
        _nextPiece = null;

        StorageManager.SaveGame();
    }

    private static List<(int, int)> GetPlacedCells(PieceData pieceData, Vector2Int pos) {
        List<(int, int)> placedCells = new();
        for (int x = 0; x < pieceData.Cells.GetLength(0); x++) {
            for (int y = 0; y < pieceData.Cells.GetLength(1); y++) {
                if (!pieceData.Cells[x, y]) {
                    continue;
                }

                Vector2Int place = new(pos.x + x, pos.y + y);
                placedCells.Add((place.x, place.y));
            }
        }

        return placedCells;
    }

    private void ShakeCamera() {
        Debug.Log("Shake camera");
    }

    private void UpdateResourceMarksAfterPlacePiece(List<(int, int)> placedCells) {
        List<int> connectedCellGroups = new List<int>();
        Color needColor = Color.clear;
        foreach (var (row, col) in placedCells) {
            if (needColor == Color.clear) {
                needColor = Instance.MainMetaConfig.CellsConfigs.First(c => c.CellType == _field[row, col]).MarkCellColor;
                needColor.a = 1;
            }

            foreach (var pos in FieldUtils.Directions) {
                var newRow = row + pos.y;
                var newCol = col + pos.x;
                if (newRow >= _field.GetLength(0) || newCol >= _field.GetLength(1) || newRow < 0 || newCol < 0 ||
                    _field[newRow, newCol] != _field[row, col]) continue;

                if (_groupCellIndex[newRow, newCol] != 0) {
                    //fix bug if piece has holes 
                    if (!connectedCellGroups.Contains(_groupCellIndex[newRow, newCol])) {
                        connectedCellGroups.Add(_groupCellIndex[newRow, newCol]);
                    }
                }
            }
        }

        foreach (var curIndex in connectedCellGroups)
        {
            CollectResourcesFromMark(curIndex - 1,1);
            _connectedGroups[curIndex - 1].ResourceMarkView.CollectAnimation();
        }

        List<(int, int)> cellsInNewGroup = new List<(int, int)>();
        Vector3 newResourceMarkPosition = new Vector3();
        int curGroupIndex = 0;
        if (connectedCellGroups.Count == 0)
            curGroupIndex = _connectedGroups.Count+1;
        else
        {
            curGroupIndex = connectedCellGroups[0];
            foreach (var pieces in _connectedGroups[curGroupIndex-1].Pieces)
            {
                cellsInNewGroup.Add((pieces.row,pieces.col));
                newResourceMarkPosition += _cells[pieces.row, pieces.col].transform.position;
            }
            if (connectedCellGroups.Count > 1)
            {
                for (int i = 1; i < connectedCellGroups.Count; i++)
                {
                   var connectedGroup = _connectedGroups[connectedCellGroups[i]-1];
                   ReleaseResourceMark(connectedGroup.ResourceMarkView);
                   foreach (var pieces in connectedGroup.Pieces)
                   {
                       _groupCellIndex[pieces.row, pieces.col] = curGroupIndex;
                       cellsInNewGroup.Add((pieces.row,pieces.col));
                       newResourceMarkPosition += _cells[pieces.row, pieces.col].transform.position;
                   }
                   _connectedGroups[connectedCellGroups[i]-1] = new ResourceMarkAndPieces(null,new List<(int,int)>());
                }
            }
        }
        
        foreach (var (row, col) in placedCells)
        {
                _groupCellIndex[row, col] = curGroupIndex;
                newResourceMarkPosition += _cells[row, col].transform.position;
                cellsInNewGroup.Add((row,col));
        }
        newResourceMarkPosition /= cellsInNewGroup.Count;
        newResourceMarkPosition += new Vector3(0, 1, 0);
        if (connectedCellGroups.Count == 0)
        {
            var resourceMarkView = SpawnResourceMark(newResourceMarkPosition, 0, 0, ResourceType.None,needColor);
            resourceMarkView.gameObject.SetActive(false);
            _connectedGroups.Add(new ResourceMarkAndPieces(resourceMarkView, cellsInNewGroup));
        }
        else
        {
            _connectedGroups[curGroupIndex - 1].ResourceMarkView.gameObject.transform.position =
                newResourceMarkPosition;
            var resourceMarkView = _connectedGroups[curGroupIndex - 1].ResourceMarkView;
          //  resourceMarkView.gameObject.SetActive(false);
            _connectedGroups[curGroupIndex - 1] = new ResourceMarkAndPieces(resourceMarkView, cellsInNewGroup);
        }

        foreach (var (row,col) in cellsInNewGroup)
        {
              StorageManager.GameDataMain.FieldRows[row].RowCells[col] = new ResourceAndCountData(_field[row, col],0);
        }
    }

    private void UpdateResourceMarks()
    {
        for (int i = 0; i < _connectedGroups.Count; i++)
        {
            if(_connectedGroups[i].ResourceMarkView == null)continue;
            int collectedResouces = 0;
            int maxCollectedResouces = 0;
            ResourceType curResource = ResourceType.None;
            foreach (var (row, col) in _connectedGroups[i].Pieces)
            {
                if(_field[row, col] == CellType.Empty)continue;
                var cellConfig = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                    (c => c.CellType == _field[row, col]);
                if (curResource == ResourceType.None)
                    curResource = cellConfig.AfkResourceType;
                if (cellConfig.AfkResourceType != ResourceType.None)
                {
                    float resourceMultiplayer = MainMetaConfig.ResourceMultipliers[_connectedGroups[i].Pieces.Count];

                    maxCollectedResouces += (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer);
                    var currentCellCollectedResources =
                        StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount
                        + (int)(cellConfig.AfkProduceCountPerSecond * resourceMultiplayer *
                                MainMetaConfig.resourceMarksUpdateCouldown);
                    currentCellCollectedResources = Mathf.Min(currentCellCollectedResources,
                        (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer));
                    collectedResouces += currentCellCollectedResources;
                    StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount =
                        currentCellCollectedResources;
                }

            }
                if (curResource != ResourceType.None)
                    _connectedGroups[i].ResourceMarkView
                        .SetResourceMarkInfo(maxCollectedResouces, collectedResouces, curResource, i);
        }
    }

    private void GetResourceCollectMarks()
    {
        List<List<(int row, int col)>> connectedGroupsPieces = null;
        (_groupCellIndex, connectedGroupsPieces) = SameCellsGroupCalculater.FindConnectedCellTypeGroups(_field);
        int afkTimeInSeconds = (int)(_currentGameTime - StorageManager.GameDataMain.LastExitTimeDateTime).TotalSeconds;
      
        
        for (int i = 0; i < connectedGroupsPieces.Count; i++)
        {
            Vector3 collectResourceMarkPosition = Vector3.zero;
            int collectedResouces = 0;
            int maxCollectedResouces = 0;
            ResourceType curResource = ResourceType.None;
            Color resourceColor = Color.clear;
            foreach (var (row, col) in connectedGroupsPieces[i])
            {
                var cellConfig = MetaManager.Instance.MainMetaConfig.CellsConfigs.First
                    (c => c.CellType == _field[row, col]);
                    
                if (curResource == ResourceType.None)
                {
                    curResource = cellConfig.AfkResourceType;
                    resourceColor = cellConfig.MarkCellColor;
                    resourceColor.a = 1;
                }
                if (cellConfig.AfkResourceType != ResourceType.None)
                {
                    float resourceMultiplayer = MainMetaConfig.ResourceMultipliers[connectedGroupsPieces[i].Count];

                    maxCollectedResouces += (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer);
                    int afkCollectedResources = StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount +
                                                (int)(afkTimeInSeconds * cellConfig.AfkProduceCountPerSecond *
                                                      resourceMultiplayer);
                    afkCollectedResources = Mathf.Min(afkCollectedResources,
                        (int)(cellConfig.MaxAfkCapacity * resourceMultiplayer));
                    StorageManager.GameDataMain.FieldRows[row].RowCells[col].ResourceCount = afkCollectedResources;
                    collectedResouces += afkCollectedResources;
                    collectResourceMarkPosition += _cells[row, col].transform.position;
                }
            }

            collectedResouces = Mathf.Min(collectedResouces, maxCollectedResouces);
            collectResourceMarkPosition /= connectedGroupsPieces[i].Count;
            var resourceMark = SpawnResourceMark(collectResourceMarkPosition, maxCollectedResouces, collectedResouces,
                curResource,resourceColor);
            resourceMark.gameObject.SetActive((float)collectedResouces / maxCollectedResouces > 0.1f);
            _connectedGroups.Add(new ResourceMarkAndPieces(resourceMark,
                connectedGroupsPieces[i]));
        }
    }

    private void DestroyChildren()
    {
        if (_cellContainer.childCount > 0)
        {
            Destroy(_cellContainer.GetChild(0).gameObject);
        }
    }
}

public struct ResourceMarkAndPieces
{
    public ResourceMarkView ResourceMarkView;
    public List<(int row, int col)> Pieces;

    public ResourceMarkAndPieces(ResourceMarkView resourceMarkView, List<(int row, int col)> pieces)
    {
        ResourceMarkView = resourceMarkView;
        Pieces = pieces;
    }
}