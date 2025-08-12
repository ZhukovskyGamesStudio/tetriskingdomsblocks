using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class SpotlightsManager : MonoBehaviour {
    public static SpotlightsManager Instance;

    [ SerializeField]
    private Transform _fingerTransform;

    [ SerializeField]
    private Image _fingerImage;
    
    [field: SerializeField]
    public Transform CenterScreenAnchor { get; private set; }

    [field: SerializeField]
    public SpotlightWithText SpotlightWithText { get; private set; }

    [field: SerializeField]
    public ShadowWithText ShadowWithText { get; private set; }

    private Tween _fingerTween;
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    
    public Tween StartFingerDragAnimation(Vector3 startPos, Vector3 endPos) {
        _fingerTransform.gameObject.SetActive(true);
        var finger = _fingerTransform.transform;
        finger.position = startPos;
        finger.localScale = Vector3.one;
        var color = _fingerImage.color;
        color.a = 0;
        _fingerImage.color = color;
        _fingerTween =  DOTween.Sequence().Append(_fingerImage.DOFade(1, 0.8f)).Join(finger.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(finger.DOMove(endPos, 2.5f)).Append(finger.DOScale(Vector3.one, 0.8f)).Join(_fingerImage.DOFade(0, 0.8f))
            .Append(finger.DOMove(startPos, 1)).SetLoops(-1, LoopType.Restart);
        return _fingerTween;
    }

    public Tween StartFingerClickAnimation(Vector3 target) {
        _fingerTransform.gameObject.SetActive(true);
        var finger = _fingerTransform.transform;
        finger.position = target;
        finger.localScale = Vector3.one;
        var color = _fingerImage.color;
        color.a = 0;
        _fingerImage.color = color;
        _fingerTween =  DOTween.Sequence().Append(_fingerImage.DOFade(1, 0.8f))
            .Join(finger.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(finger.DOScale(Vector3.one, 0.8f))
            .Join(_fingerImage.DOFade(0, 0.8f))
            .AppendInterval(1).SetLoops(-1, LoopType.Restart);
        return _fingerTween;
    }

    public void HideFinger() {
        _fingerTween?.Kill();
        _fingerTransform.gameObject.SetActive(false);
    }
}