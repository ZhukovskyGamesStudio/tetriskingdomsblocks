using UnityEngine;

public class RandomRotatedChildObjectsView : MonoBehaviour
{
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            int randomY = Random.Range(0, 360);
            child.localRotation = Quaternion.Euler(new Vector3(
                child.localRotation.x,
                randomY,
                child.localRotation.z
            ));
            child.gameObject.isStatic = true;
            Debug.Log(child.name + " lrotated" + randomY);
        }
    }
}
