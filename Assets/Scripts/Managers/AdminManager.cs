using UnityEngine;

public class AdminManager : MonoBehaviour {
    public void ShowCollectAllDialog() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }

    public void AddResources() {
        for (int i = 0; i < 3; i++) {
            StorageManager.GameDataMain.resourcesCount[i] += 1000;
        }
        MetaManager.Instance.UpdateResourcesCountUIText();
    }
}