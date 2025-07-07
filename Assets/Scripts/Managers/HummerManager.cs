using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HummerManager : MonoBehaviour
{
   public static HummerManager Instance;
   private Tween _hummerTween;
   private Sequence _hummerSequence;
   [SerializeField]
   private Transform _hummerContainer;
   [SerializeField]
   private Transform _hummerContainerStart;
   [SerializeField]
   private Transform _hummerContainerEnd;

   [SerializeField] private TMP_Text _hummerText;
   

   private void Awake()
   {
      Instance = this;
   }

   public void HummerDestroyPieceAnimation(Vector3 cellPosition)
   {
      _hummerSequence.Kill();
      _hummerSequence = DOTween.Sequence();

      _hummerSequence
         .Append(_hummerContainer.transform.DOMove(
            new Vector3(cellPosition.x + 1, cellPosition.y, cellPosition.z), 0.8f))
         .Append(_hummerContainer.transform.DORotate(new Vector3(0, 0, 90f), 0.2f))
         .Append(_hummerContainer.transform.DORotate(new Vector3(0, 0, 0f), 0.2f))
         .Append(_hummerContainer.transform.DOMove(_hummerContainerStart.position, 0.8f)).OnComplete(() =>
         {
            if (GameFieldManager.Instance != null && StorageManager.GameDataMain.HummerCount <= 0)
               HideHummerAnimation();
            else
            {
               Tween floatTween = _hummerContainer.DOMoveY(_hummerContainer.position.y + 1, 0.5f)
                  .SetLoops(-1, LoopType.Yoyo);
               _hummerSequence.Append(floatTween);
            }
         });
   }

   public void ShowHummerAnimation()
   {
      if (_hummerText != null)
         _hummerText.text = "Cancel";

      _hummerSequence.Kill();
      _hummerSequence = DOTween.Sequence();

      _hummerSequence.Append(_hummerContainer.DOMove(_hummerContainerStart.position, 0.8f));

      Tween floatTween = _hummerContainer.DOMoveY(_hummerContainer.position.y + 1, 0.5f).SetLoops(1000, LoopType.Yoyo);

      _hummerSequence.Append(floatTween);
   }

   public void HideHummerAnimation()
   {
      _hummerSequence.Kill();

      if (_hummerText != null)
         _hummerText.text = "Destroy pieces mode";
      
      _hummerSequence.Append(_hummerContainer.transform.DOMove(_hummerContainerEnd.position, 0.8f));
   }
}
