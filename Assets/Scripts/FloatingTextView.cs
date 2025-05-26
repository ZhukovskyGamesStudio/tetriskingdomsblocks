using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingTextView : MonoBehaviour
{
    [SerializeField] private TMP_Text floatingText;
    [SerializeField] private Animator floatingTextAnimator;
    private float currentYPosTarget;


    public void SetText(Vector2 newPosition, string newText, float textSize)
    {
        gameObject.SetActive(true);
        floatingText.transform.position = newPosition;
        floatingText.color = Color.white;
        floatingText.fontSize = textSize;
        //floatingTextAnimator.SetTrigger("SetText");
        floatingText.text = newText;
        currentYPosTarget = newPosition.y + 150;
        StartCoroutine(MoveText());
        Invoke("HideText", 2f);
    }

    public void HideText() => GameManager.Instance.ReleaseFloatingText(this);

    public IEnumerator MoveText()//mb change to dotween
    {
        while (currentYPosTarget > floatingText.rectTransform.anchoredPosition.y)
        {
            floatingText.rectTransform.anchoredPosition = new Vector2(floatingText.rectTransform.anchoredPosition.x, floatingText.rectTransform.anchoredPosition.y + Time.deltaTime * 50);
            floatingText.color = new Color(1f, 1f, 1f, floatingText.color.a - Time.deltaTime * 0.5f);
             yield return null;
        }
        HideText();
    }
}
