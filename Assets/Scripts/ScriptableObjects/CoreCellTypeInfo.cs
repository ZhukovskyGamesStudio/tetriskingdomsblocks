using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CoreCellInfo", menuName = "Scriptable Objects/CoreCellInfo")]
public class CoreCellTypeInfo : CellTypeInfo {
    public ResourceTypeAndCountSubClass[] ResourcesForPlace;

    public ResourceTypeAndCountSubClass[] ResourcesForDestroy;

    public int MultiplayerForSameResourceType;
}