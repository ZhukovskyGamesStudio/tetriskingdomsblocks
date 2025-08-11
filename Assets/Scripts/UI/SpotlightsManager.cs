using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class SpotlightsManager : MonoBehaviour {
    public static SpotlightsManager Instance;

    [field: SerializeField]
    public Image FingerTransform { get; private set; }
    
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
    
    public Tween StartFingerAnimation(Vector3 startPos, Vector3 endPos) {
        FingerTransform.gameObject.SetActive(true);
        var finger = FingerTransform.transform;
        finger.position = startPos;
        finger.localScale = Vector3.one;
        var color = FingerTransform.color;
        color.a = 0;
        FingerTransform.color = color;
        _fingerTween =  DOTween.Sequence().Append(FingerTransform.DOFade(1, 0.8f)).Join(finger.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(finger.DOMove(endPos, 2.5f)).Append(finger.DOScale(Vector3.one, 0.8f)).Join(FingerTransform.DOFade(0, 0.8f))
            .Append(finger.DOMove(startPos, 1)).SetLoops(-1, LoopType.Restart);
        return _fingerTween;
    }

    public void HideFinger() {
        _fingerTween?.Kill();
        FingerTransform.gameObject.SetActive(false);
    }
}