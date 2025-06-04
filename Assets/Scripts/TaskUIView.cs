using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TaskUIView : MonoBehaviour
{
   [FormerlySerializedAs("currentTaskInfo")] public TMP_Text CurrentTaskInfo;

   [FormerlySerializedAs("taskInfoTextHelper")] public SpawnedForOneCharTextView TaskInfoTextHelper;
  // public TMP_Text currentTaskValue;
   //public Slider filledBarImage;
   [FormerlySerializedAs("checkBox")] [field:SerializeField]
   private Transform _checkBox;
   private Tween _currentTween;
   
   public void AddTextAnimation(/*float needSliderValue*/)
   {
      _currentTween.Kill();
      _currentTween = DOTween.Sequence()    
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 1.5f, 0.3f)) 
            .Join(_checkBox.transform.DOScale(Vector3.one, 0.2f))
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 0.95f, 0.2f))
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one, 0.07f));
         
        // .Join(filledBarImage.DOValue(needSliderValue,0.4f))
     
   }

   public void CompleteTask()
   {
      CurrentTaskInfo.fontStyle = FontStyles.Strikethrough;
      _currentTween.Kill();
      _currentTween = DOTween.Sequence()
         .Append(_checkBox.transform.DOScale(Vector3.one, 0.3f))
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 1.5f, 0.3f)) 
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 0.95f, 0.2f))
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one, 0.07f));
   }
}
