using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarsConfig", menuName = "Scriptable Objects/AvatarsConfig")]
public class AvatarsConfig : ScriptableObject {
    public List<Sprite> PossibleAvatars;
}
