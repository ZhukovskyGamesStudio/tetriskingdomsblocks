using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoseDialog : DialogBase {
    [SerializeField]
    private TextMeshProUGUI _hpText;
    
    private Action _clickRetry, _clickExit;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickRetry = dialogData.ClickRetry;
        _clickExit = dialogData.ClickExit;
        _hpText.text = _hpText.text.Replace("{hp}", dialogData.Hp.ToString());
    }

    public override UniTask Show(Action onClose) {
        LessMusic();
        return base.Show(onClose);
    }

    private void LessMusic() {
        BackgroundMusicManager.Instance.SetMusicVolume(0f);
    }

    private void MoreMusic() {
        BackgroundMusicManager.Instance.SetMusicVolume(1f);
    }
    public void ClickRetry() {
        _clickRetry.Invoke();
        MoreMusic();
        Hide().Forget();
    }

    public void ClickExit() {
        _clickExit.Invoke();
        MoreMusic();
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public Action ClickRetry, ClickExit;
        public int Hp;
    }
}