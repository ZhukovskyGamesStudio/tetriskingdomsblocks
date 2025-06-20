using UnityEngine;

//[DefaultExecutionOrder(-10000)]
public class RuntimeStateResetter : MonoBehaviour {
    private void Awake() {
        /*foreach (GameObject resetable in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if (resetable.GetComponent<IResetable>() != null) {
                resetable.GetComponent<IResetable>().Reset();
            }
        }*/
    }
}