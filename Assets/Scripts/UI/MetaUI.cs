using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MetaUI : MonoBehaviour {
    public static MetaUI Instance;
    [field: SerializeField]
    public Transform _filtersContainer { get; private set; }
    [field: SerializeField]
    public Transform _destroyPiecesButtonTransform { get; private set; }
    [field: SerializeField]
    public Button CloseInventoryButton { get; private set; }
    [field: SerializeField]
    public Button BuildButton { get; private set; }

    [field: SerializeField]
    public HealthView HealthView { get; private set; }

    [field: SerializeField]
    public CountersPanelView CountersPanelView { get; private set; }

    [SerializeField]
    private TMP_Text _destroyPieceText;

    [field: SerializeField]
    public Transform _openResourceTabButtonTransform { get; private set; }

    [SerializeField]
    private TMP_Text _playText;

    [SerializeField]
    private GameObject _ruleState, _buildState, _hammerState;

    [SerializeField]
    private GameObject _ruleCamera, _buildCamera;

    [SerializeField]
    private GameObject _buyPieceButton;

    [field: SerializeField]
    public RectTransform _mainCanvas { get; set; }

    private Vector3 _buildCameraShift;

    [SerializeField]
    private Image _ruleAvatarImage;

    [SerializeField]
    private AvatarsConfig _avatarsConfig;

    [field: SerializeField]
    public GetPieceButtonView GetPieceButtonView { get; private set; }

    [field: SerializeField]
    public Button PlayButton { get; private set; }

    [SerializeField]
    public MetaTutorial _metaTutorial;

    [SerializeField]
    public Transform _metaTutorialContainer;

    [field: SerializeField]
    public Button CraftButton { get; private set; }
    [field: SerializeField]
    public CanvasGroup CountersCanvasGroup { get; private set; }

    [field: SerializeField]
    public Button ProfileButton { get; private set; }

    [field: SerializeField]
    public Button ResourcesButton { get; private set; }

    [field: SerializeField]
    public Button SettingsButton { get; private set; }
    
    [field: SerializeField]
    public Button BuyPieceButton { get; private set; }

    [field: SerializeField]
    public GameObject Tabs { get; private set; }

    public bool IsBuildState;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        SetAvatar(StorageManager.GameDataMain.ProfileAvatar);
        InitBuildCameras();
        CraftButton.gameObject.SetActive(MainManager.Instance._mainConfig.SawmillUnlockLevel <= StorageManager.GameDataMain.CurMaxLevel);
        //TODO Skipped for testing purposes
    }

    private void InitBuildCameras() {
        var ray = new Ray(_buildCamera.transform.position, _buildCamera.transform.forward);
        var hit = Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Ground"));

        if (hit) {
            _buildCameraShift = _buildCamera.transform.position - hitinfo.point;
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
        MetaFieldManager.Instance.CanInteractWithField(false);
        DialogsManager.Instance.ShowDialogWithData(dialogData);
    }

    public void OpenLootboxDialog(PieceData rewardingPiece) {
        var dialogData = new DialogWithData {
            DialogType = typeof(LootboxDialog),
            Data = new LootboxDialog.Data {
                RewardingPiece = rewardingPiece
            }
        };
        MetaFieldManager.Instance.CanInteractWithField(false);
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
        MetaFieldManager.Instance.CanInteractWithField(false);
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
      
        if (resourcesInfo.Count == 0) {
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
                    new OverviewResourceInfo(ResourceType.Coins, (int)StorageManager.GameDataMain.GetResource(ResourceType.Coins),
                        (int)(resourcesInfo[ResourceType.Coins] * 3600)),
                    new OverviewResourceInfo(ResourceType.Wood, (int)StorageManager.GameDataMain.GetResource(ResourceType.Wood),
                        (int)(resourcesInfo[ResourceType.Wood] * 3600)),
                    new OverviewResourceInfo(ResourceType.Rocks, (int)StorageManager.GameDataMain.GetResource(ResourceType.Rocks),
                        (int)(resourcesInfo[ResourceType.Rocks] * 3600)),
                    new OverviewResourceInfo(ResourceType.Food, (int)StorageManager.GameDataMain.GetResource(ResourceType.Food),
                        (int)(resourcesInfo[ResourceType.Food]) * 3600),
                    new OverviewResourceInfo(ResourceType.Metal, (int)StorageManager.GameDataMain.GetResource(ResourceType.Metal),
                        (int)(resourcesInfo[ResourceType.Metal] * 3600))
                },
                ShowResource = StorageManager.GameDataMain.SeenResource
            }
        };

        MetaFieldManager.Instance.CanInteractWithField(false);
        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    public void SetPlayText(string text) {
        _playText.text = text;
    }

    public void UpdateGetPieceTimer(TimeSpan timeLeft) {
        GetPieceButtonView.UpdateGetPieceTimer(timeLeft);
    }

    public void OpenBuildState() {
        IsBuildState = true;
        _buildState.SetActive(true);
        _ruleState.SetActive(false);
        _ruleCamera.SetActive(false);
        _buildCamera.SetActive(true);
        
        MetaFieldManager.Instance.CanOpenLockedZones = false;

        MetaFieldManager.Instance.CloseCellUI();
        var ray = new Ray(_ruleCamera.transform.position, _ruleCamera.transform.forward);
        var hit = Physics.Raycast(ray, out RaycastHit hitinfo, 100, LayerMask.GetMask("Ground"));

        if (hit) {
            _buildCamera.transform.position = hitinfo.point + _buildCameraShift;
        }

        MetaWorldCanvasView.Instance.gameObject.SetActive(false);
    }

    public void CloseBuildState() {
        IsBuildState = false;
        _buildState.SetActive(false);
    }

    public void OpenHammerState() {
        _hammerState.gameObject.SetActive(true);
        CloseBuildState();
    }

    public void CloseHammerState() {
        _hammerState.gameObject.SetActive(false);
        OpenBuildState();
    }

    public void OpenRuleState() {
        IsBuildState = false;
        MetaFieldManager.Instance.CanOpenLockedZones = true;
        DialogsManager.Instance.CloseAllDialogs();
        _buildState.SetActive(false);
        _ruleState.SetActive(true);
        _ruleCamera.SetActive(true);
        _buildCamera.SetActive(false);
        MetaWorldCanvasView.Instance.gameObject.SetActive(true);
    }

    public void OpenShop(bool onPiece) {
        _ruleState.SetActive(false);
        _buildState.SetActive(false);
        var dialog = new DialogWithData {
            DialogType = typeof(RealShopDialog),
            Data = new RealShopDialog.Data {
                ClickClose = MetaTabsPanel.Instance.OpenRule,
                BuyResource = MainManager.Instance.BuyMetaResource,
                BuyOffer = MainManager.Instance.BuyBundleOffer,
                BuyPieceForCoins = MainManager.Instance.BuyPiece,
                OnPiece = onPiece
            }
        };

        DialogsManager.Instance.ShowDialogWithData(dialog);
    }

    public void OpenSettings() {
        MetaFieldManager.Instance.CanOpenLockedZones = false;
        MetaFieldManager.Instance.CanDragCamera = false;
        SettingsManager.Instance.ShowMetaSettingsDialog();
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(_ruleCamera.transform.position, _ruleCamera.transform.position + _ruleCamera.transform.forward * 30);
        Gizmos.DrawLine(_buildCamera.transform.position, _buildCamera.transform.position + _buildCamera.transform.forward * 30);
    }
}