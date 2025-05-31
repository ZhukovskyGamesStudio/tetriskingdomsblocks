using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingTextView : MonoBehaviour
{
    [SerializeField] private TMP_Text floatingText;
    [SerializeField] private Animator floatingTextAnimator;
    private float currentYPosTarget;
    public Tween currentTween;

    public void SetText(Vector2 newPosition, string newText, float textSize, float showTime)
    {
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        floatingText.transform.position = newPosition;
        floatingText.color = Color.white;
        floatingText.fontSize = textSize;
        //floatingTextAnimator.SetTrigger("SetText");
        floatingText.text = newText;
        currentYPosTarget = newPosition.y + 150;
        //StartCoroutine(MoveText());
        MoveUpText(showTime);
        Invoke("HideText", showTime);
    }

    public void HideText() => GameManager.Instance.ReleaseFloatingText(this);


    public void MoveUpText(float showTime)
    {
        currentTween.Kill();
        currentTween = DOTween.Sequence()
            .Append(transform.DOMoveY(transform.position.y + 150, showTime))
            .Join(transform.DOScale(transform.localScale * 1.5f, showTime-0.2f));
    }
   /* public IEnumerator MoveText()//mb change to dotween
    {
        while (currentYPosTarget > floatingText.rectTransform.anchoredPosition.y)
        {
            floatingText.rectTransform.anchoredPosition = new Vector2(floatingText.rectTransform.anchoredPosition.x, floatingText.rectTransform.anchoredPosition.y + Time.deltaTime * 50);
            floatingText.color = new Color(1f, 1f, 1f, floatingText.color.a - Time.deltaTime * 0.5f);
             yield return null;
        }
        HideText();
    }*/

    public void OnDestroy()
    {
        currentTween.Kill();
    }
}
