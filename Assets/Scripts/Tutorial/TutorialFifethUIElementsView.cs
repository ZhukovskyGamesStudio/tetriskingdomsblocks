using UnityEngine.UI;

public class TutorialHammerView : BoosterTutorialView {
    protected override void Init() {
        StorageManager.GameDataMain.ResourcesCount[ResourceType.HammerBooster] = 5;

        if (BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }

    protected override Button BoosterButton => GameUI.Instance.GameBoostersButtons._hummerButton;
}