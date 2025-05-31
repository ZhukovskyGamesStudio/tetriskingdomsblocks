using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUIView : MonoBehaviour
{
   public TMP_Text currentTaskInfo;
   public TMP_Text currentTaskValue;
   public Slider filledBarImage;
   private Tween currentTween;
   
   public void AddTextAnimation()
   {
      currentTween.Kill();
      currentTween = DOTween.Sequence()
         .Append(currentTaskValue.transform.DOScale(Vector3.one * 1.5f, 0.3f))
         .Append(currentTaskValue.transform.DOScale(Vector3.one * 0.95f, 0.2f))
         .Append(currentTaskValue.transform.DOScale(Vector3.one, 0.07f));
   }
}
