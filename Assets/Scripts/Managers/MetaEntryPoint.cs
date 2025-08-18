using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MetaEntryPoint : MonoBehaviour {
    [SerializeField]
    private MetaFieldManager _metaFieldManager;

    private void Start() {
        InitScene().Forget();
    }

    private async UniTask InitScene() {
        VibrationsManager.Instance.StopAll();
        IconRendererManager.Instance.InitializeRenderSystem();
        _metaFieldManager.SetupGame();
        CameraScaleToBounds.Instance.Init();
        await UniTask.WaitWhile(() => CameraScaleToBounds.Instance.IsInited);
        if (StorageManager.GameDataMain.PlacedInMetaPiecesCount == 0 && !AdminManager.Instance.IsSkipTutorials) {
            CameraScaleToBounds.Instance.MoveCameraToStartingPosition();
            Instantiate(MetaUI.Instance._metaTutorial, MetaUI.Instance._metaTutorialContainer);
        } else {
            //CameraScaleToBounds.Instance.MoveCameraToStartingPosition();
            CameraScaleToBounds.Instance.MoveCameraToVillagePosition();
        }

        (int cubes, int coins) = MainManager.Instance.GetRewardToMeta();

        FloatingResourcesManager.Instance.FromPointToPointAnimation(coins, ResourceType.Coins, MetaUI.Instance.PlayButton.transform.position,
            MetaUI.Instance.CountersPanelView.GetCoinsIconPosition, MetaFieldManager.Instance.ChangeResorceText,
            StorageManager.GameDataMain.GetResource(ResourceType.Coins), false, true, false, false);
        FloatingResourcesManager.Instance.FromPointToPointAnimation(cubes, ResourceType.MagicCube,
            MetaUI.Instance.PlayButton.transform.position, MetaUI.Instance.CountersPanelView.GetMagicCubesIconPosition,
            MetaFieldManager.Instance.ChangeResorceText, StorageManager.GameDataMain.GetResource(ResourceType.MagicCube), false, true, false,
            false);
    }
}