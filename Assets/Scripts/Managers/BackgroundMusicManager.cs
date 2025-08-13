using Cysharp.Threading.Tasks;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour {
    [SerializeField]
    private AudioQueueMixer _audioQueueMixer;

    public static BackgroundMusicManager Instance;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async UniTaskVoid PlayEndlessMusic() {
        while (true) {
            await _audioQueueMixer.PlayNextBlended();
        }
    }

    public void SetMusicVolume(float multiplier) {
        _audioQueueMixer.VolumeMultiplier = multiplier;
    }

    public void ChangeIsPlayingMusic(bool isOn) {
        _audioQueueMixer.StopCurrentAudioSource(isOn);
    }
}