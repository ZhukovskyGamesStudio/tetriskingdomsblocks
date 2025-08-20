using Cysharp.Threading.Tasks;
using Google.Play.Review;
using UnityEngine;

public class MadPixelRateUsProvider : IRateUsProvider {
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo;

    public UniTask Show() {
        return LaunchReviewFlow();
    }

    private async UniTask LaunchReviewFlow() {
        _reviewManager = new ReviewManager();
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        await requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError) {
            Debug.LogError(requestFlowOperation.Error.ToString());
            return;
        }

        _playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        await launchFlowOperation;
        _playReviewInfo = null; // Сброс объекта

        if (launchFlowOperation.Error != ReviewErrorCode.NoError) {
            Debug.LogError(launchFlowOperation.Error.ToString());
        }
    }
}