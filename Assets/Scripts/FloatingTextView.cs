using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class FloatingTextView : MonoBehaviour
{
    [FormerlySerializedAs("floatingText")] [SerializeField] private TMP_Text _floatingText;
    [FormerlySerializedAs("floatingTextAnimator")] [SerializeField] private Animator _floatingTextAnimator;
    private Tween _currentTween;

    public void SetText(Vector2 newPosition, string newText, float textSize, float showTime)
    {
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        _floatingText.transform.position = newPosition;
        _floatingText.color = Color.white;
        _floatingText.fontSize = textSize;
        _floatingText.text = newText;
        MoveUpText(showTime);
        Invoke("HideText", showTime);
    }

    public void HideText() => GameManager.Instance.ReleaseFloatingText(this);


    public void MoveUpText(float showTime)
    {
        _currentTween.Kill();
        _currentTween = DOTween.Sequence()
            .Append(transform.DOMoveY(transform.position.y + 150, showTime))
            .Join(transform.DOScale(transform.localScale * 1.5f, showTime-0.2f));
    }

    public void OnDestroy()
    {
        _currentTween.Kill();
    }
}
