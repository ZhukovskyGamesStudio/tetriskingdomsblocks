#if APPMETRICA
using Io.AppMetrica;
using UnityEngine;

public static class AppMetricaActivator {
    private const string API_KEY = "e11ee2f6-613d-48a6-bc13-3f1d298fe901";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Activate() {
        AppMetrica.Activate(new AppMetricaConfig(API_KEY) {
            FirstActivationAsUpdate = !IsFirstLaunch(),
        });
    }

    private static bool IsFirstLaunch() {
        // Implement logic to detect whether the app is opening for the first time.
        // For example, you can check for files (settings, databases, and so on),
        // which the app creates on its first launch.
        return !StorageManager.HasSavedGame();
    }
}
#endif