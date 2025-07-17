using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIElementsView : MonoBehaviour
{
    [SerializeField] private RectTransform[] _holeImages;
    [SerializeField] private RectTransform _blackBGImage;
    [SerializeField] private RectTransform _fingerImage;
    [SerializeField] private Tween _currentTween;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       TutorialFieldManager.Instance.OnCellPlaced += DestroyTutorial;
     //   foreach (var hole in _holeImages) {
     //       hole.SetParent( GameUI.Instance.HolesForBgContainer);
      //  }
      //  _blackBGImage.SetParent(GameUI.Instance.BlackBgContainer);
      //  _fingerImage.SetParent(GameUI.Instance.BlackBgContainer);
        StartAnimation();
    }

    public void SetHolesPositions(Vector3 posHoleFirst, Vector3 posHoleSecond) {
        posHoleFirst = (Vector2)Camera.main.WorldToScreenPoint(posHoleFirst);
        posHoleSecond = (Vector2)Camera.main.WorldToScreenPoint(posHoleSecond);
        // Присваиваем позицию UI-элементу
        _holeImages[0].transform.position= posHoleFirst;
        _fingerImage.transform.position = _holeImages[0].transform.position;
        _holeImages[1].transform.position = posHoleSecond;
    }

    public void StartAnimation()
    {
        _currentTween = DOTween.Sequence()
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[1].transform.position, 2.5f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[0].transform.position, 1))
            .Append(_fingerImage.DOScale(Vector3.one * 0.75f, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[1].transform.position, 2.5f))
            .Append(_fingerImage.DOScale(Vector3.one, 0.8f))
            .Append(_fingerImage.DOMove(_holeImages[0].transform.position, 1))
            .SetLoops(-1, LoopType.Restart);
        //.Append(_fingerImage.DOMove(transform.localScale * 1.5f, 3));
    }
    
    void StrechImageToFullSreen()
    {
       /* RectTransform rectTransform = _blackBGImage;
        
        rectTransform.anchorMin = Vector2.zero; // (0, 0)
        rectTransform.anchorMax = Vector2.one;   // (1, 1)
        rectTransform.offsetMin = Vector2.zero;  // left, bottom
        rectTransform.offsetMax = Vector2.zero;  // right, top
        rectTransform.localScale = Vector3.one;

        _holeImages[1].anchoredPosition = new Vector3(0, 890, 0);
        _holeImages[0].anchoredPosition = new Vector3(-50, -1195, 0);
        _fingerImage.position = _holeImages[0].position;
        _holeImages[0].localScale = Vector3.one;
        _holeImages[1].localScale = Vector3.one;*/
    }

    public void DestroyTutorial()
    {
        _currentTween.Kill();
        foreach (var hole in _holeImages)
        {
            Destroy(hole.gameObject); 
        }

        Destroy(_blackBGImage.gameObject);
        Destroy(_fingerImage.gameObject);
        
        Destroy(gameObject);
        TutorialFieldManager.Instance.OnCellPlaced -= DestroyTutorial;
    }
}
