using System;
using DG.Tweening;
using UnityEngine;

public class CellView : MonoBehaviour {
   // [field: SerializeField]
//    public CellType CellType { get; private set; }

    [SerializeField]
    private GameObject _objectsContainer;

    private Tween _currentTween;
    private void Start() {
        if (_objectsContainer) {
            RandomRotateObjects();
        }
    }

    private void RandomRotateObjects() {
        _objectsContainer.transform.Rotate(Vector3.up, UnityEngine.Random.Range(0f, 360f));
    }

    public void PlaceCellOnField()
    {
        _currentTween = DOTween.Sequence()
            .Append(transform.DOScale(transform.localScale * 1.1f, 0.25f))
            .Append(transform.DOScale(transform.localScale * 1f, 0.2f));
    }
    public void DestroyCell()
    {
        Destroy(gameObject, 0.8f);
        _currentTween.Kill();
        _currentTween = DOTween.Sequence()
            .Append(transform.DOScale(transform.localScale * 1.2f, 0.2f))
            .Append(transform.DOScale(transform.localScale * 0f, 0.4f));
    }

    private void OnDestroy()
    {
        _currentTween.Kill();
    }
}