using System;
using UnityEngine;

public class MetaEntryPoint : MonoBehaviour {

    [SerializeField]
    private MetaFieldManager _metaFieldManager;
    private void Start() {
        _metaFieldManager.SetupGame();

       (int cubes, int coins) = MainManager.Instance.GetRewardToMeta();
       UIAnimationsUtils.FromPointToPointAnimation(coins, ResourceType.Coins,
           MetaUI.Instance._playButton.transform.position ,MetaUI.Instance.CountersPanelView.GetCoinsPosition,
      MetaFieldManager.Instance.ChangeResorceText,StorageManager.GameDataMain.GetResource(ResourceType.Coins), false );
       UIAnimationsUtils.FromPointToPointAnimation(cubes, ResourceType.MagicCube,
           MetaUI.Instance._playButton.transform.position ,MetaUI.Instance.CountersPanelView.GetMagicCubesPosition,
           MetaFieldManager.Instance.ChangeResorceText,StorageManager.GameDataMain.GetResource(ResourceType.MagicCube), false);
    }
}
