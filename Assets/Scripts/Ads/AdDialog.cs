using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AdDialog : DialogBase {
    [SerializeField]
    private Animation _adAnimation;

    [SerializeField]
    private AnimationClip _adClip;

    public override async UniTask Show(Action onClose) {
        await base.Show(onClose);
        _adAnimation.Play(_adClip.name);
        await UniTask.WaitWhile(() => _adAnimation.isPlaying);
        Hide();
    }

    public void OnClick() {
        if (AdsManager.Instance.IsCanSkipAds) {
            Hide();
        }
    }
}