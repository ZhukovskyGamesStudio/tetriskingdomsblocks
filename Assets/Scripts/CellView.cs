using System;
using DG.Tweening;
using UnityEngine;

public class CellView : MonoBehaviour {

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
            .Append(transform.DOScaleY(transform.localScale.y * 0.6f, 0.25f))
            .Join(transform.DOScaleX(transform.localScale.x * 1.1f, 0.25f))
            .Join(transform.DOScaleZ(transform.localScale.z * 1.1f, 0.25f))
            .Append(transform.DOScaleY(transform.localScale.y * 1.2f, 0.2f))
            .Join(transform.DOScaleX(transform.localScale.x * 0.8f, 0.2f))
            .Join(transform.DOScaleZ(transform.localScale.z * 0.8f, 0.2f))
            .Append(transform.DOScale(new Vector3(1,0.25f,1), 0.25f));
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