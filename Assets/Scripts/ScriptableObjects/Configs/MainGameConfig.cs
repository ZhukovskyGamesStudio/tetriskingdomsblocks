using UnityEngine;

[CreateAssetMenu(fileName = "MainGameConfig", menuName = "Scriptable Objects/MainGameConfig")]
public class MainGameConfig : ScriptableObject
{
    public bool resourceOnPlaceCell;
    public bool resourceOnDestroyCell;
    public bool bonusResourcesOnDestroyLine;

    public int minResourcesToTask;
    public int maxResourcesToTask;

    public int fieldSize;

}
