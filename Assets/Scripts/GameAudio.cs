using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameAudio : MonoBehaviour {
    public static GameAudio Instance { get; private set; }
    [Header("Piece Placement Sounds")]
    public AudioQueueMixer PlacePiece;
    public AudioQueueMixer PiecesAppear;

    [Header("Break Sounds")]
    public AudioQueueMixer BoxBreaks;
    public AudioQueueMixer IceBreaks;
    public AudioQueueMixer SlimeBreaks;

    [Header("Material Placement Sounds")]
    public AudioQueueMixer WoodPlaced;
    public AudioQueueMixer RockPlaced;
    public AudioQueueMixer WheatPlaced;
    public AudioQueueMixer MetalPlaced;
    public AudioQueueMixer HousePlaced;
    
    [Header("Cubes use on meta")]
    public AudioQueueMixer CubesStart;
    public AudioQueueMixer CubesMiddle;
    public AudioQueueMixer CubesEnd;
    public AudioQueueMixer CloudsRemove;

    [Header("Game Events")]
    public AudioQueueMixer Win;
    public AudioQueueMixer Lose;
    public AudioQueueMixer ResourceCollected;
    public AudioQueueMixer RowCollected;
    public AudioQueueMixer UseHammer;
    public AudioQueueMixer UseDynamite;
    public AudioQueueMixer UseShuffle;

    private void Awake() {
        Instance = this;
    }

    public void PlayNextSound(AudioQueueMixer mixer)
    {
        if (!StorageManager.GameDataMain.SettingsData.IsSoundOn) {
            return;
        }
        mixer.PlayNext();
    }

    public async UniTask PlayNextSoundWithDelay(AudioQueueMixer mixer, float delay) {
        if (!StorageManager.GameDataMain.SettingsData.IsSoundOn) {
            return;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        mixer.PlayNext();
    }
}