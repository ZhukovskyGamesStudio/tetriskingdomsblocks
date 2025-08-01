using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FloatingTextView : MonoBehaviour
{
    [FormerlySerializedAs("floatingText")] [SerializeField] private Image _floatingImage;
    [FormerlySerializedAs("floatingTextAnimator")] [SerializeField] private Animator _floatingTextAnimator;
    private Tween _currentTween;

    public void SetText(Vector2 newPosition, Sprite newSprite, float spriteSize, float showTime, Vector2 finalPosition)
    {
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        _floatingImage.transform.position = newPosition;
        _floatingImage.sprite = newSprite;
        _floatingImage.rectTransform.sizeDelta = new Vector2(spriteSize, spriteSize);
        MoveUpText(showTime, finalPosition);
        if (finalPosition != Vector2.zero)
            Invoke(nameof(HideText), showTime + 1.5f);
        else
            Invoke(nameof(HideText), showTime);
    }

    public void HideText() {
        GameFieldManager.Instance.ReleaseFloatingText(this);
    } 

    public void MoveUpText(float showTime, Vector2 finalPosition)
    {
        _currentTween.Kill();
        float rnd = Random.Range(0.5f, 0.75f);
        if (finalPosition != Vector2.zero) {
            _currentTween = DOTween.Sequence().Append(transform.DOMoveY(transform.position.y + 150, showTime))
                .Join(transform.DOScale(transform.localScale * 1.5f, showTime - 0.2f)).Append(transform.DOMove(finalPosition, rnd))
                .Join(transform.DOScale(Vector3.zero, rnd))
                .OnComplete(() => { GameFieldManager.Instance.PlayCollectedSound(); });

        }     else
            _currentTween = DOTween.Sequence()
                .Append(transform.DOMoveY(transform.position.y + 150, showTime))
                .Join(transform.DOScale(transform.localScale * 1.5f, showTime - 0.2f));
    }

    public void OnDestroy()
    {
        _currentTween.Kill();
    }
}
