using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUIView : MonoBehaviour
{
   public TMP_Text currentTaskInfo;

   public SpawnedForOneCharTextView taskInfoTextHelper;
  // public TMP_Text currentTaskValue;
   //public Slider filledBarImage;
   [field:SerializeField]
   private Transform checkBox;
   private Tween currentTween;
   
   public void AddTextAnimation(/*float needSliderValue*/)
   {
      currentTween.Kill();
      currentTween = DOTween.Sequence()    
         .Append(currentTaskInfo.transform.DOScale(Vector3.one * 1.5f, 0.3f)) 
            .Join(checkBox.transform.DOScale(Vector3.one, 0.2f))
         .Append(currentTaskInfo.transform.DOScale(Vector3.one * 0.95f, 0.2f))
         .Append(currentTaskInfo.transform.DOScale(Vector3.one, 0.07f));
         
        // .Join(filledBarImage.DOValue(needSliderValue,0.4f))
     
   }

   public void CompleteTask()
   {
      currentTaskInfo.fontStyle = FontStyles.Strikethrough;
      currentTween.Kill();
      currentTween = DOTween.Sequence()
         .Append(checkBox.transform.DOScale(Vector3.one, 0.3f))
         .Append(currentTaskInfo.transform.DOScale(Vector3.one * 1.5f, 0.3f)) 
         .Append(currentTaskInfo.transform.DOScale(Vector3.one * 0.95f, 0.2f))
         .Append(currentTaskInfo.transform.DOScale(Vector3.one, 0.07f));
   }
}
