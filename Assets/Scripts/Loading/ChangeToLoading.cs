using UnityEngine.SceneManagement;

public static class ChangeToLoading {
    public static void TryChange() {
        if (!LoadingManager.IsLoaded) {
            SceneManager.LoadScene("Loading");
        }
    }
}