using UnityEngine;

public class AdminManager : MonoBehaviour {
    public void ShowCollectAllDialog() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }
}