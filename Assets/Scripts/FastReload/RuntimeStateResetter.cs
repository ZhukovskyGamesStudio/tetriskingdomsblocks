using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class RuntimeStateResetter : MonoBehaviour {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() {
        foreach (GameObject resetable in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            if ((resetable.GetComponent<IResetable>() != null)) {
            //    resetable.GetComponent<IResetable>().Reset();
            }
        }
    }
}