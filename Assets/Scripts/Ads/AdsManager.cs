using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AdsManager : MonoBehaviour {
    public static AdsManager Instance { get; private set; }

    [SerializeField]
    private Animation _adAnimation;

    [SerializeField]
    private AnimationClip _adClip;

    [field:SerializeField]
    public bool IsCanSkipAds { get; private set; }

    private bool _isAdSkipped;

    private void Awake() {
        Instance = this;
    }

    public async UniTask ShowRewarded(Action onAdEnded) {
        _isAdSkipped = false;
        _adAnimation.gameObject.SetActive(true);
        _adAnimation.Play(_adClip.name);
        await UniTask.WaitWhile(() => _adAnimation.isPlaying && !_isAdSkipped);
        _adAnimation.gameObject.SetActive(false);
        onAdEnded?.Invoke();
    }

    public void OnClickedAd() {
        if (Instance.IsCanSkipAds) {
            _isAdSkipped = true;
        }
    }
}