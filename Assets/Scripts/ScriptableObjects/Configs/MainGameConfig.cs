using UnityEngine;

[CreateAssetMenu(fileName = "MainGameConfig", menuName = "Scriptable Objects/MainGameConfig")]
public class MainGameConfig : ScriptableObject {
    public bool resourceOnPlaceCell;
    public bool resourceOnDestroyCell;
    public bool bonusResourcesOnDestroyLine;

    public int NeededUltimatePoints;
    public int MaxUltimateCells;
    public int[] LinesCountMultiplayers;

    public int FieldSize;
}