using UnityEngine;

[CreateAssetMenu(fileName = "BoostersConfig", menuName = "Scriptable Objects/BoostersConfig")]
public class BoostersConfig : ScriptableObject
{
    public int DynamiteRadius;
    public CellTypeInfo DinamyteCellInfo;

    public float PieceRotationSpeed;
    
    public Sprite LockBoosterSprite;

    public int DynamiteUnlockLevel;
    public int RandomUnlockLevel;
    public int RotateUnlockLevel;
    public int HummerUnlockLevel;
}
