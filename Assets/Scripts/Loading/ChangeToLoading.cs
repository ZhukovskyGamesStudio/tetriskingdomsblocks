using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10000)]
public class ChangeToLoading : MonoBehaviour {
    private void Awake() {
        TryChange();
    }

    private void TryChange() {
        if (LoadingManager.Instance == null) {
            SceneManager.LoadScene("LoadingScene");
            return;
        }

        if (!LoadingManager.Instance.IsLoaded) {
            SceneManager.LoadScene("LoadingScene");
            return;
        }

        Destroy(gameObject);
    }
}