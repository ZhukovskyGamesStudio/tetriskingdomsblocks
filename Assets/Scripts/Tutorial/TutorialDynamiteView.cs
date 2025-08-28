using System.Collections.Generic;
using UnityEngine.UI;

public class TutorialDynamiteView : BoosterTutorialView {
    protected override void Init() {
        StorageManager.GameDataMain.ResourcesCount[ResourceType.BombBooster] = 5;

        if (BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }
    
    protected override void SendTutorialEventStep() {
        ZhukovskyAnalyticsManager.Instance.SendCustomEvent("tutorial", new Dictionary<string, object> {
            { "step_name", "_dynamiteTutorial"  }
        }, true);
    }

    protected override Button BoosterButton => GameUI.Instance.GameBoostersButtons._dinamyteButton;
}