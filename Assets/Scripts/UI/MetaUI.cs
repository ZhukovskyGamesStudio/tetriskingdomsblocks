using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Pool;
using UnityEngine.UI;

public class MetaUI : MonoBehaviour {
    public static MetaUI Instance;

    [field: SerializeField]
    public HealthView HealthView { get; private set; }

    [field: SerializeField]
    public CountersPanelView CountersPanelView { get; private set; }

    [SerializeField]
    private GameObject _getPieceTimer;
    
    [SerializeField]
    private TMP_Text _getPieceTimerText;

    [SerializeField]
    private TMP_Text _destroyPieceText;

    [SerializeField]
    private TMP_Text _playText;

    [SerializeField]
    private GameObject _ruleState, _buildState;

    [SerializeField]
    private GameObject _ruleCamera, _buildCamera;

    [SerializeField]
    private GameObject _getPieceButton, _buyPieceButton;

    [field: SerializeField]
    public RectTransform _mainCanvas { get;set; }

    private Vector3 _buildCameraShift;

    [SerializeField]
    private TMP_Text _floatingTextPrefab;
    
    [SerializeField]
    private Transform _floatingTextContainer;

    [SerializeField]
    private Image _ruleAvatarImage;

    [SerializeField]
    private AvatarsConfig _avatarsConfig;

    private ObjectPool<TMP_Text> _floatingTextsPool;

    private void Awake() {
        Instance = this;
        _floatingTextsPool = new ObjectPool<TMP_Text>(() => Instantiate(_floatingTextPrefab, _floatingTextContainer));
        InitBuildCameras();
    }
    
    public TMP_Text ShowFloatingText() {
        var floatingText = _floatingTextsPool.Get();
        floatingText.gameObject.SetActive(true);
        return floatingText;
    }

    public void ReleaseFloatingText(TMP_Text needTextObject) {
        needTextObject.gameObject.SetActive(false);
        _floatingTextsPool.Release(needTextObject);
    }

    public void ShowRetentionDialog() {
        if(!MainManager.Instance._hasInternetConnection)return;
        LoadingManager.Instance.FirstLoad = false;
        var afkResources = MetaFieldManager.Instance.GetAllAfkResourceInfoForDialog();
        afkResources.TryAdd(ResourceType.Wood, 0);
        afkResources.TryAdd(ResourceType.Rocks, 0);
        afkResources.TryAdd(ResourceType.Food, 0);
        afkResources.TryAdd(ResourceType.Metal, 0);
        var dialog = new DialogWithData {
            DialogType = typeof(RetentionDialog),
            Data = new RetentionDialog.Data {
                ClickDoubleClaim = MetaFieldManager.Instance.CollectDoubleResourcesFromAllMarks,
                OfflineResources = new List<RetentionDialog.RetentionResource> {
                    new RetentionDialog.RetentionResource { Count = (int)afkResources[ResourceType.Wood], Resource = ResourceType.Wood },
                    new RetentionDialog.RetentionResource { Count = (int)afkResources[ResourceType.Rocks], Resource = ResourceType.Rocks },
                    new RetentionDialog.RetentionResource { Count = (int)afkResources[ResourceType.Food], Resource = ResourceType.Food },
                    new RetentionDialog.RetentionResource { Count = (int)afkResources[ResourceType.Metal], Resource = ResourceType.Metal }
                }
                
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    private void InitBuildCameras() {
        var ray = new Ray(_buildCamera.transform.position, _buildCamera.transform.forward);
        var hit = Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Ground"));

        if (hit) {
            _buildCameraShift =  _buildCamera.transform.position - hitinfo.point;
        }
    }

    public void OpenProfile() {
        var dialog = new DialogWithData {
            DialogType = typeof(ProfileDialog),
            Data = new ProfileDialog.Data {
                BuiltCells = 123,
                Levels = 123,
                WeeksBest = 123, // TODO: убрать заглушки
                Wins = 123,
                PlayerName = "PlayerName12345",
                ClickEditAvatar = OpenEditAvatar,
                AvatarSprite = _avatarsConfig.PossibleAvatars[StorageManager.GameDataMain.ProfileAvatar]
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    public void OpenEditAvatar() {
        var dialog = new DialogWithData {
            DialogType = typeof(EditAvatarDialog),
            Data = new EditAvatarDialog.Data {
                PlayerName = "PlayerName12345",
                ClickClose = OpenProfile,
                ClickChangeAvatar = SetAvatar,
                PossibleAvatars = _avatarsConfig.PossibleAvatars,
                CurrentAvatar = StorageManager.GameDataMain.ProfileAvatar
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    public void SetAvatar(int avatarId) {
        StorageManager.GameDataMain.ProfileAvatar = avatarId;
        _ruleAvatarImage.sprite = _avatarsConfig.PossibleAvatars[avatarId];
    }

    public void OpenResources() {
        Dictionary<ResourceType, float> resourcesInfo = MetaFieldManager.Instance.GetAllResourceInfoForDialog();
        resourcesInfo.TryAdd(ResourceType.Wood,  0);
        resourcesInfo.TryAdd(ResourceType.Rocks,  0);
        resourcesInfo.TryAdd(ResourceType.Food,  0);
        var dialog = new DialogWithData {
            DialogType = typeof(OverviewDialog),
            Data = new OverviewDialog.Data {
                Resources = new List<OverviewResourceInfo> {
                    new OverviewResourceInfo(ResourceType.Wood, (int)StorageManager.GameDataMain.ResourcesCount[0], (int)resourcesInfo[ResourceType.Wood], 0),
                    new OverviewResourceInfo(ResourceType.Rocks, (int)StorageManager.GameDataMain.ResourcesCount[1], (int)resourcesInfo[ResourceType.Rocks], 0),
                    new OverviewResourceInfo(ResourceType.Food, (int)StorageManager.GameDataMain.ResourcesCount[2], (int)resourcesInfo[ResourceType.Food], 0)
                   // new OverviewResourceInfo(ResourceType.Rocks, 12345, resourcesInfo[ResourceType.Wood].income, 0)
                }
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    public void SetPlayText(string text) {
        _playText.text = text;
    }

    public void SetGetPieceButtonActive(bool isActive) {
        _getPieceTimer.SetActive(!isActive);
        _buyPieceButton.SetActive(!isActive);
        
        _getPieceButton.SetActive(isActive);
    }

    public void UpdateGetPieceTimer(TimeSpan timeLeft) {
        if (timeLeft.TotalSeconds > 0) {
            _getPieceTimerText.text = TimeConverter.ConvertToTimeString(timeLeft);
        }
        else if (_getPieceTimer.activeSelf) {
            SetGetPieceButtonActive(true);
        }
    }

    public void OpenBuildState() {
        _buildState.SetActive(true);
        _ruleState.SetActive(false);
        _ruleCamera.SetActive(false);
        _buildCamera.SetActive(true);

        var ray = new Ray(_ruleCamera.transform.position, _ruleCamera.transform.forward);
        var hit = Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Ground"));

        if (hit) {
            _buildCamera.transform.position = hitinfo.point + _buildCameraShift;
        }

        MetaWorldCanvasView.Instance.gameObject.SetActive(false);
    }

    public void OpenRuleState() {
        DialogsManager.Instance.CloseAllDialogs();
        _buildState.SetActive(false);
        _ruleState.SetActive(true);
        _ruleCamera.SetActive(true);
        _buildCamera.SetActive(false);
        MetaWorldCanvasView.Instance.gameObject.SetActive(true);
    }

    public void OpenShop() {
        _ruleState.SetActive(false);
        var dialog = new DialogWithData {
            DialogType = typeof(RealShopDialog),
            Data = new RealShopDialog.Data {
                ClickClose = MetaTabsPanel.Instance.OpenRule
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    public void OpenSettings() {
        SettingsManager.Instance.ShowMetaSettingsDialog();
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(_ruleCamera.transform.position, _ruleCamera.transform.position + _ruleCamera.transform.forward * 30);
        Gizmos.DrawLine(_buildCamera.transform.position, _buildCamera.transform.position + _buildCamera.transform.forward * 30);
    }
}