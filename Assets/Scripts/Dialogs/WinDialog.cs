using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class WinDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _coinsText, _cubesText;

    private Action _clickClaim;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickClaim = dialogData.ClickClaim;
        _coinsText.text = dialogData.Coins.ToString();
        _cubesText.text = dialogData.Cubes.ToString();
    }

    public override UniTask Show(Action onClose) {
        LessMusic();
        return base.Show(onClose);
    }

    private void LessMusic() {
        GameAudio.Instance.PlayNextSound(GameAudio.Instance.Win);
        BackgroundMusicManager.Instance.SetMusicVolume(0f);
    }

    private void MoreMusic() {
        BackgroundMusicManager.Instance.SetMusicVolume(1f);
    }

    public void ClickClaim() {
        _clickClaim.Invoke();
        Hide().Forget();
        MoreMusic();
    }

    [Serializable]
    public class Data {
        public Action ClickClaim;
        public int Coins;
        public int Cubes;
    }
}