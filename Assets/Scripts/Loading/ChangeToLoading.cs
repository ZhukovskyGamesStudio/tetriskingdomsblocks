using UnityEngine.SceneManagement;

public static class ChangeToLoading {
    public static void TryChange() {
        if(LoadingManager.Instance == null) {
            SceneManager.LoadScene("LoadingScene");
            return;
        }
        if (!LoadingManager.Instance.IsLoaded) {
            SceneManager.LoadScene("LoadingScene");
        }
    }
}