using UnityEngine.UI;

public class TutorialShuffleView : BoosterTutorialView {
    protected override void Init() {
        StorageManager.GameDataMain.RandomFieldCount = 5;

        if (BoostersManager.Instance != null) {
            GameUI.Instance.GameBoostersButtons.UpdateCounters(StorageManager.GameDataMain);
        }
    }

    protected override Button BoosterButton => GameUI.Instance.GameBoostersButtons.ShuffleButton;
}