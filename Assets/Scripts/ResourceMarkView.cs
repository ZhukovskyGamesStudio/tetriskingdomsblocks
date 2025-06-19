using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMarkView : MonoBehaviour
{
    [SerializeField] private TMP_Text _resourceMarkText;
    [SerializeField] private Image _resourceFillImage;
    [SerializeField] private int markIndex;
    [SerializeField] private Button _buttonMark;
    private Tween _floatTween;
    private bool _isAnimate;

    public void SetResourceMarkInfo(int maxResource, int currentResource, ResourceType resourceType, int index)
    {
        markIndex = index;
        if ((float)currentResource / maxResource > 0.1f)
        {
            gameObject.SetActive(true);
        _resourceMarkText.text = currentResource + "/"+maxResource + "\n <sprite name=" + resourceType + ">";
        _resourceFillImage.fillAmount = (float)currentResource / maxResource;
        }

        if (gameObject.activeInHierarchy && !_isAnimate)
        {
            Sequence sequence = DOTween.Sequence();
            _isAnimate = true;
            sequence.Append(_resourceFillImage.transform.DOScale(1f, 0.3f));

            _floatTween = _resourceFillImage.transform.DOScale(0.9f, 0.5f)
                .SetLoops(1000, LoopType.Yoyo);
            sequence.Append(_floatTween);
        }
    }

    public void CollectResources()
    {
        MetaManager.Instance.CollectResourcesFromMark(markIndex);
        CollectAnimation();
    }

    public void CollectAnimation()
    {
        _buttonMark.enabled = false;
        _isAnimate = false;
        _floatTween.Kill();
        _floatTween = DOTween.Sequence().Append(_resourceFillImage.transform.DOScale(1.1f, 0.3f))
            .Append(_resourceFillImage.transform.DOScale(0f, 0.7f)).OnComplete(() =>
            {
                gameObject.SetActive(false);
                _buttonMark.enabled = true;
                _floatTween.Complete();
            });
    }
}
