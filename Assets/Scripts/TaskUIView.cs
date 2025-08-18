using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TaskUIView : MonoBehaviour
{
   [FormerlySerializedAs("currentTaskInfo")] public TMP_Text CurrentTaskInfo;
   public Image TaskImage;
   public Image TaskSubImage;

   [FormerlySerializedAs("taskInfoTextHelper")] public SpawnedForOneCharTextView TaskInfoTextHelper;
   private Tween _currentTween;

   [SerializeField]
   private GameObject _checkmark;

   public void SetData(TaskInfoSubClass task) {
      gameObject.SetActive(true);
      switch (task.TaskType) {
         case TaskInfo.TaskType.getResource:
            break;

         case TaskInfo.TaskType.placeMonoLine:
            TaskSubImage.sprite = SpritesManager.Instance.LineSprite;
            break;
      }

      TaskImage.sprite = SpritesManager.Instance.GetSprite(task.NeedResource);
      TaskInfoTextHelper.SetText(task.Count.ToString());
      
   }
   
   
   public void AddTextAnimation()
   {
      _currentTween.Kill();
      _currentTween = DOTween.Sequence()    
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 1.5f, 0.3f)) 
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 0.95f, 0.2f))
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one, 0.07f));
   }

   public void CompleteTask()
   {
    //  CurrentTaskInfo.fontStyle = FontStyles.Strikethrough;
      _currentTween.Kill();
      _currentTween = DOTween.Sequence()
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 1.5f, 0.3f)) 
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one * 0.95f, 0.2f))
         .Append(CurrentTaskInfo.transform.DOScale(Vector3.one, 0.07f))
         .OnComplete(() => {
            _checkmark.SetActive(true);
         });
   }
}
