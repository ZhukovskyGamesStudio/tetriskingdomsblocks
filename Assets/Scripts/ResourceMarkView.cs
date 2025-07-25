using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceMarkView : MonoBehaviour
{
    [SerializeField] private TMP_Text _resourceMarkText;
    [SerializeField] private Image _resourceFillImage;
    public int markIndex { get;set; }
    [SerializeField] private Button _buttonMark;
    private Tween _floatTween;
    private bool _isAnimate;

    private void Start()
    {
        _buttonMark.onClick.AddListener(() =>CollectResources());
    }

    public void SetResourceMarkInfo(int maxResource, float currentResource, ResourceType resourceType, int index)
    {
        markIndex = index;
        if (currentResource / maxResource > 0.1f)
        {
            gameObject.SetActive(true);
        _resourceMarkText.text = Mathf.FloorToInt(currentResource) + "\n <sprite name=" + resourceType + ">";
        _resourceFillImage.fillAmount = currentResource / maxResource;
        }

        if (gameObject.activeInHierarchy && !_isAnimate)
        {
            Sequence sequence = DOTween.Sequence();
            _isAnimate = true;
            sequence.Append(transform.DOScale(1f, 0.3f));

            _floatTween = transform.DOScale(0.9f, 0.5f)
                .SetLoops(1000, LoopType.Yoyo);
            sequence.Append(_floatTween);
        }
    }

    public void SetColor(Color color)
    {
        _resourceFillImage.color = color;
    }
    public void CollectResources()
    {
        MetaFieldManager.Instance.CollectResourcesFromMark(markIndex,1);
        CollectAnimation();
    }

    public void CollectAnimation()
    {
        if(!_resourceFillImage.gameObject.activeInHierarchy)return;
        _buttonMark.enabled = false;
        _isAnimate = false;
        _floatTween.Kill();
        _floatTween = DOTween.Sequence().Append(transform.DOScale(1.1f, 0.3f))
            .Append(transform.DOScale(0f, 0.7f)).OnComplete(() =>
            {
                gameObject.SetActive(false);
                _buttonMark.enabled = true;
                _floatTween.Complete();
            });
    }
}
