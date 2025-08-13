using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.Pool;
using UnityEngine.UI;

public class MetaUI : MonoBehaviour {
    public static MetaUI Instance;
    [field: SerializeField]
    public Transform _buildButton { get;private set; }
    [field: SerializeField]
    public HealthView HealthView { get; private set; }

    [field: SerializeField]
    public CountersPanelView CountersPanelView { get; private set; }

    [SerializeField]
    private TMP_Text _destroyPieceText;

    [field:SerializeField]
    public Transform _openResourceTabButtonTransform{ get; private set; }
    
    [SerializeField]
    private TMP_Text _playText;

    [SerializeField]
    private GameObject _ruleState, _buildState;

    [SerializeField]
    private GameObject _ruleCamera, _buildCamera;

    [SerializeField]
    private GameObject _buyPieceButton;

    [field: SerializeField]
    public RectTransform _mainCanvas { get;set; }

    private Vector3 _buildCameraShift;


    [SerializeField]
    private Image _ruleAvatarImage;

    [SerializeField]
    private AvatarsConfig _avatarsConfig;
    
    [field:SerializeField]
    public GetPieceButtonView  _getPieceButtonView{ get; private set; }
    [field:SerializeField]
    public Button  _playButton{ get; private set; }
    [SerializeField]
    private MetaTutorial  _metaTutorial;
    [SerializeField]
    private Transform  _metaTutorialContainer;

    public bool IsBuildState;

    private void Awake() {
        Instance = this;
        SetAvatar(StorageManager.GameDataMain.ProfileAvatar);
        InitBuildCameras();
        //TODO Skipped for testing purposes
       if (StorageManager.GameDataMain.PlacedInMetaPiecesCount == 0 && !AdminManager.Instance.IsSkipTutorials) {
           Instantiate(_metaTutorial, _metaTutorialContainer);
       }
    }
    

    private void InitBuildCameras() {
        var ray = new Ray(_buildCamera.transform.position, _buildCamera.transform.forward);
        var hit = Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Ground"));

        if (hit) {
            _buildCameraShift =  _buildCamera.transform.position - hitinfo.point;
        }
    }

    public void OpenCraftsDialog() {
        var dialogData = new DialogWithData {
            DialogType = typeof(MetaCraftDialog),
            Data = new MetaCraftDialog.Data {
                Crafts = ConfigsManager.Instance.MetaCraftsConfig.Crafts,
                Craft = MetaFieldManager.Instance.Craft
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialogData);
    }

    public void OpenLootboxDialog(PieceData rewardingPiece) {
        var dialogData = new DialogWithData {
            DialogType = typeof(LootboxDialog),
            Data = new LootboxDialog.Data {
                RewardingPiece = rewardingPiece
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialogData);
    }

    public void OpenProfile() {
        var dialog = new DialogWithData {
            DialogType = typeof(ProfileDialog),
            Data = new ProfileDialog.Data {
                BuiltCells = StorageManager.GameDataMain.PlacedInMetaPiecesCount,
                Levels = StorageManager.GameDataMain.CurMaxLevel,
                WeeksBest = 123, // TODO: убрать заглушки
                Wins = StorageManager.GameDataMain.FirstAttemptWinLevelsCount,
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
        if(resourcesInfo.Count == 0) {
            return;
        }
        resourcesInfo.TryAdd(ResourceType.Wood, 0);
        resourcesInfo.TryAdd(ResourceType.Rocks, 0);
        resourcesInfo.TryAdd(ResourceType.Food, 0);
        resourcesInfo.TryAdd(ResourceType.Metal, 0);
        var dialog = new DialogWithData {
            DialogType = typeof(OverviewDialog),
            Data = new OverviewDialog.Data {
                Resources = new List<OverviewResourceInfo> {
                    new OverviewResourceInfo(ResourceType.Coins, (int)StorageManager.GameDataMain.GetResource(ResourceType.Coins), (int)resourcesInfo[ResourceType.Coins]),
                    new OverviewResourceInfo(ResourceType.Wood, (int)StorageManager.GameDataMain.GetResource(ResourceType.Wood), (int)resourcesInfo[ResourceType.Wood]),
                    new OverviewResourceInfo(ResourceType.Rocks, (int)StorageManager.GameDataMain.GetResource(ResourceType.Rocks), (int)resourcesInfo[ResourceType.Rocks]),
                    new OverviewResourceInfo(ResourceType.Food, (int)StorageManager.GameDataMain.GetResource(ResourceType.Food), (int)resourcesInfo[ResourceType.Food]),
                    new OverviewResourceInfo(ResourceType.Metal, (int)StorageManager.GameDataMain.GetResource(ResourceType.Metal), (int)resourcesInfo[ResourceType.Metal])
                },
                ShowResource = StorageManager.GameDataMain.SeenResource
            }
        };
        
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    public void SetPlayText(string text) {
        _playText.text = text;
    }

    public void UpdateGetPieceTimer(TimeSpan timeLeft) {
        _getPieceButtonView.UpdateGetPieceTimer(timeLeft);
    }

    public void OpenBuildState() {
        IsBuildState = true;
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
        IsBuildState = false;
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
                ClickClose = MetaTabsPanel.Instance.OpenRule,
                BuyResource = MainManager.Instance.BuyMetaResource
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