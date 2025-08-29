using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour {
    [SerializeField]
    private AudioQueueMixer _audioQueueMixer;

    public static BackgroundMusicManager Instance;

    private CancellationTokenSource _cts;
    private CancellationTokenSource _linkedCts;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _cts = new CancellationTokenSource();
    }

    public async UniTaskVoid StopAndPlayEndlessMusic() {
        if (_cts != null) {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy(), _cts.Token);
        }

        while (!_linkedCts.IsCancellationRequested) {
            await _audioQueueMixer.PlayNextBlended(_linkedCts.Token);
        }
    }

    public void SetMusicVolume(float multiplier) {
        _audioQueueMixer.VolumeMultiplier = multiplier;
    }

    public void ChangeIsPlayingMusic(bool isOn) {
        _audioQueueMixer.StopCurrentAudioSource(isOn);
    }
}