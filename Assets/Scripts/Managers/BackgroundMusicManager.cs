using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour {
    [SerializeField]
    private AudioQueueMixer _audioQueueMixer;

    private void Awake() {
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
}