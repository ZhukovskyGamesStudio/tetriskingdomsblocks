using System;
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

    private void Start() {
        PlayEndlessMusic().Forget();
    }

    private async UniTaskVoid PlayEndlessMusic() {
        while (true) {
            await _audioQueueMixer.PlayNextBlended();
        }
    }

    public void ChangeIsPlayingMusic(bool isOn)
    {
        _audioQueueMixer.StopCurrentAudioSource(isOn);
    }
}