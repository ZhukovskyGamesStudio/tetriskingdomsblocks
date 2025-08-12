using System;
using UnityEngine;

public class MetaEntryPoint : MonoBehaviour {

    [SerializeField]
    private MetaFieldManager _metaFieldManager;
    private void Start() {
        _metaFieldManager.SetupGame();
       (int cubes, int coins) = MainManager.Instance.GetRewardToMeta();
       
       FloatingResourcesManager.Instance.FromPointToPointAnimation(coins, ResourceType.Coins,
           MetaUI.Instance._playButton.transform.position ,MetaUI.Instance.CountersPanelView.GetCoinsIconPosition,
      MetaFieldManager.Instance.ChangeResorceText,StorageManager.GameDataMain.GetResource(ResourceType.Coins), false, true );
       FloatingResourcesManager.Instance.FromPointToPointAnimation(cubes, ResourceType.MagicCube,
           MetaUI.Instance._playButton.transform.position ,MetaUI.Instance.CountersPanelView.GetMagicCubesIconPosition,
           MetaFieldManager.Instance.ChangeResorceText,StorageManager.GameDataMain.GetResource(ResourceType.MagicCube), false, true);
       
    }
}
