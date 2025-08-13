using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MetaTutorial : MonoBehaviour {
    [SerializeField]
    private SpotlightAnimConfig _metaTutor0, _metaTutor1, _metaTutor2, _metaTutor3, _metaTutor4;

    [SerializeField]
    private RectTransform _holeImageGetFreeTetramineButton;

    [SerializeField]
    private RectTransform _holeImageContinue;

    [SerializeField]
    private RectTransform _holeImageBuildButton;

    [SerializeField]
    private RectTransform _holeTetraminesToBuild;

    [SerializeField]
    private List<Vector3Int> _openedCloudCells, _secondStepCells, _thirdStepCells;

    [SerializeField]
    private bool _canSkipTutorial;

    private int _tutorialStep = 1;

    private Vector3 _cameraPosition = new Vector3(-7.5f, 0, -2);

    private EventTrigger _invCell;
    private GameObject _pieceCellsContainer;

    void Start() {
        // GameFieldManager.Instance.OnCellPlaced += HideFirstStepTutorial;
        TryAddMissingResources();
        TutorialAsync().Forget();
    }

    private void TryAddMissingResources() {
        if (StorageManager.GameDataMain.GetResource(ResourceType.MagicCube) <= 25) {
            StorageManager.GameDataMain.SetResource(ResourceType.MagicCube, 25);
            Debug.LogWarning("Missing cubes in tutorial!");
        }
    }

    private void DisableUI() {
        MetaUI.Instance.CraftButton.gameObject.SetActive(false);
        MetaUI.Instance.ProfileButton.gameObject.SetActive(false);
        MetaUI.Instance.ResourcesButton.gameObject.SetActive(false);
        MetaUI.Instance.Tabs.gameObject.SetActive(false);
    }

    public void EnableUI() {
        MetaUI.Instance.CraftButton.gameObject.SetActive(false);
        MetaUI.Instance.ProfileButton.gameObject.SetActive(true);
        MetaUI.Instance.ResourcesButton.gameObject.SetActive(true);
        MetaUI.Instance.Tabs.gameObject.SetActive(true);
    }

    private async UniTask TutorialAsync() {
        DisableUI();
        MetaUI.Instance._playButton.enabled = false;
        ShowFirstStepTutorial();
        SetHolesPositions();
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        MoveCameraToNeedPosition();
        await UniTask.WaitUntil(() => MetaWorldCanvasView.Instance.UnlockFieldCellsView.gameObject.activeInHierarchy);
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        ShowSecondStepTutorial();
    }

    public void SetHolesPositions() {
        _holeTetraminesToBuild.gameObject.SetActive(false);
        _holeImageBuildButton.transform.SetParent(MetaUI.Instance.BuildButton.transform);
        _holeImageBuildButton.transform.localPosition = Vector3.zero;
        _holeImageBuildButton.gameObject.SetActive(false);
        _holeImageGetFreeTetramineButton.SetParent(MetaUI.Instance._getPieceButtonView.transform);
        _holeImageGetFreeTetramineButton.transform.localPosition = Vector3.zero;
        _holeImageGetFreeTetramineButton.gameObject.SetActive(false);
        MetaUI.Instance.BuildButton.enabled = false;
        MetaUI.Instance._getPieceButtonView.GetPieceButton.GetComponent<Button>().enabled = false;
    }

    private void ShowFirstStepTutorial() {
        //MoveCameraToNeedPosition();
        //Invoke(nameof(MoveCameraToNeedPosition), 1f);
        TutorialHoleHelper.DestroyHoles();
        TutorialHoleHelper.SpawnHoles(_openedCloudCells, false);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(SpotlightsManager.Instance.CenterScreenAnchor, _metaTutor0);
        SpotlightsManager.Instance.StartFingerClickAnimation((Vector2)Camera.main.WorldToScreenPoint(new Vector3(4f, 0, 2f)));
    }

    private void MoveCameraToNeedPosition() {
        MetaFieldManager.Instance.CanDragCamera = false;
        MetaFieldManager.Instance.CameraContainer.position += _cameraPosition;
    }

    public void ShowSecondStepTutorial() {
        TutorialHoleHelper.HighlightObjects(new List<GameObject> { _pieceCellsContainer });
        TutorialHoleHelper.SpawnHoles(_secondStepCells);
        _tutorialStep = 2;
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlightOnButton(MetaWorldCanvasView.Instance.UnlockFieldCellsView.UnlockButton,
            _metaTutor1, HideSecondStepTutorial);
    }

    public void HideSecondStepTutorial() {
        HideSecondStepTutorialAsync().Forget();
    }

    private async UniTask HideSecondStepTutorialAsync() {
        TutorialHoleHelper.DestroyHoles();
        SpotlightsManager.Instance.HideFinger();
        bool waiting = true;
        FloatingResourcesManager.Instance.OnAnimationEnd += _ => waiting = false;
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        await UniTask.WaitWhile(() => waiting);
        FloatingResourcesManager.Instance.OnAnimationEnd -= _ => waiting = false;
        ShowThirdStepTutorial();
    }

    public void ShowThirdStepTutorial() {
        _tutorialStep = 3;
        _holeImageGetFreeTetramineButton.gameObject.SetActive(true);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlightOnButton(MetaUI.Instance._getPieceButtonView.GetPieceButton, _metaTutor2,
            HideThirdStepTutorial);

        MetaUI.Instance._getPieceButtonView.GetPieceButton.GetComponent<Button>().enabled = true;
    }

    private void HideThirdStepTutorial() {
        HideThirdStepTutorialAsync().Forget();
    }

    private async UniTask HideThirdStepTutorialAsync() {
        _holeImageGetFreeTetramineButton.gameObject.SetActive(false);
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        await UniTask.WaitWhile(() => StorageManager.GameDataMain.InventoryFigures.Count == 0 || DialogsManager.Instance.IsDialogActive);
        ShowFourthStepTutorial();
    }

    private void ShowFourthStepTutorial() {
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlightOnButton(MetaUI.Instance.BuildButton, _metaTutor3, HideFourthStepTutorial);
        _holeImageBuildButton.gameObject.SetActive(true);

        MetaUI.Instance.BuildButton.enabled = true;
        _tutorialStep = 4;
    }

    private void HideFourthStepTutorial() {
        HideFourthStepTutorialAsync().Forget();
    }

    private async UniTask HideFourthStepTutorialAsync() {
        await SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        ShowFifthStepTutorial();
    }

    private void ShowFifthStepTutorial() {
        TutorialHoleHelper.SpawnHoles(_openedCloudCells);
        SpotlightsManager.Instance.SpotlightWithText.ShowSpotlight(SpotlightsManager.Instance.CenterScreenAnchor, _metaTutor4);
        _holeImageBuildButton.gameObject.SetActive(true);
        _tutorialStep = 5;

        Invoke(nameof(SetupInventoryCell), 0.3f);
    }

    private void SetupInventoryCell() {
        _invCell = GameObject.Find("InventoryCell(Clone)")?.GetComponent<EventTrigger>();
        SpotlightsManager.Instance.StartFingerDragAnimation(_invCell.transform.position,
            (Vector2)Camera.main.WorldToScreenPoint(new Vector3(4f, 0, 3.5f)));

        _holeTetraminesToBuild.gameObject.SetActive(true);
        _holeTetraminesToBuild.position = _invCell.transform.position;
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener(ShowSixthStepTutorial);

        _invCell.triggers.Add(entry);
    }

    private void ShowSixthStepTutorial(BaseEventData eventData) {
        _tutorialStep = 5;

        MetaFieldManager.Instance.OnCellPlaced += (i, bools) => HideSixthStepTutorial();
    }

    private void HideSixthStepTutorial() {
        SpotlightsManager.Instance.SpotlightWithText.HideSpotlight();
        MetaFieldManager.Instance.OnCellPlaced -= (_, _) => HideSixthStepTutorial();
        MetaFieldManager.Instance.CanDragCamera = true;
        TutorialHoleHelper.DestroyHoles();
        _holeImageBuildButton.gameObject.SetActive(false);
        DestroyTutorial();
    }

    public void DestroyTutorial() {
        EnableUI();
        MetaUI.Instance._playButton.enabled = true;
        StorageManager.GameDataMain.IsTutorialComplete = true;
        StorageManager.SaveGame();
        Destroy(gameObject);
    }
}