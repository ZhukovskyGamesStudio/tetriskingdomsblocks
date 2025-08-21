using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zhukovsky;

public class RecoveryHealthDialog : DialogBase
{
    public void RecoveryHealthWithAds()
    {
        if (StorageManager.GameDataMain.HealthCount > 0)
            MainManager.Instance.Restart();
        else
        {
            Hide().Forget();
            AdsManager.Instance.ShowRewarded(() =>
            {
                StorageManager.GameDataMain.HealthCount++;
                SceneManager.LoadScene("GameScene");
            }).Forget();
        }
    }
}
