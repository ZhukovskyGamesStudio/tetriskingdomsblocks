using UnityEngine;

public class GameAudio: MonoBehaviour {
  
    public AudioQueueMixer PlacePiece, PiecesAppear;
    public AudioQueueMixer BoxBreaks, IceBreaks, SlimeBreaks;
    public AudioQueueMixer WoodPlaced, RockPlaced, WheatPlaced, MetalPlaced;
    public AudioQueueMixer Win, Lose, ResourceCollected, RowCollected;

    public void PlayNextSound(AudioQueueMixer mixer)
    {
       if(!StorageManager.GameDataMain.IsSoundOn) return;
        mixer.PlayNext();
    }
}
