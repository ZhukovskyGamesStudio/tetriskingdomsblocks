using UnityEngine;

public class GameAudio : MonoBehaviour
{
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

    [Header("Game Events")]
    public AudioQueueMixer Win;
    public AudioQueueMixer Lose;
    public AudioQueueMixer ResourceCollected;
    public AudioQueueMixer RowCollected;

    public void PlayNextSound(AudioQueueMixer mixer)
    {
        if (!StorageManager.GameDataMain.SettingsData.IsSoundOn) return;
        mixer.PlayNext();
    }
}