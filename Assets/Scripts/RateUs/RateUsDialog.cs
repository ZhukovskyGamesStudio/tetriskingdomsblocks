using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;

public class RateUsDialog : DialogBase {
    private bool _isWaitingReview;

    public void RateGood() {
        if (_isWaitingReview) {
            return;
        }

        _isWaitingReview = true;
        SendRateUsEvent(5);
        StorageManager.GameDataMain.WasRated = true;
        StorageManager.SaveGame();
        LaunchReviewFlow().Forget();
    }

    public void RateBad() {
        SendRateUsEvent(1);
        StorageManager.GameDataMain.LastTimeRateUsShowed = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        StorageManager.SaveGame();
        HideByButton();
    }

    public void CloseWithEvent() {
        SendRateUsEvent(0);
        HideByButton();
    }

    private async UniTask LaunchReviewFlow() {
        await RateUsManager.Instance.Provider.Show();
        HideByButton();
    }

    private void SendRateUsEvent(int rateResult) {
        ZhukovskyAnalyticsManager.Instance.SendCustomEvent("rate_us", new Dictionary<string, object> {
            { "show_reason", RateUsManager.Instance.RateUsSource },
            { "rate_result", rateResult }
        });
        MetaFieldManager.Instance.CanInteractWithField(true);
        RateUsManager.Instance.RateUsSource = null;
    }
}