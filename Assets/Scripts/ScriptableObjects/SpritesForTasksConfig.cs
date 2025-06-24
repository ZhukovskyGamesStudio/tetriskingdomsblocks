using UnityEngine;

[CreateAssetMenu(fileName = "SpritesForTasksConfig", menuName = "Scriptable Objects/SpritesForTasksConfig")]
public class SpritesForTasksConfig : ScriptableObject
{
    public NameAndImage[] NameAndImages;
    public Sprite LineSprite;
    public Sprite PlaceCellSprite;
}
