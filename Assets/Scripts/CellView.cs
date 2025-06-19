using System;
using DG.Tweening;
using UnityEngine;

public class CellView : MonoBehaviour {
    [SerializeField]
    private GameObject _objectsContainer;
    [SerializeField]
    private Collider _cellCollider;
    private Tween _currentTween;
    public Guid Seed { get; private set; } = Guid.NewGuid();

    public void SetSeed(Guid seed) {
        Seed = seed;
        if (_objectsContainer) {
            RandomRotateObjects(Seed);
        }
    }

    private void RandomRotateObjects(Guid seed) {
        int hash = seed.GetHashCode();
        UnityEngine.Random.InitState(hash);

        float angle = UnityEngine.Random.Range(0f, 360f);
        _objectsContainer.transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    public void PlaceCellOnField() {
        return;
        _currentTween = DOTween.Sequence().Append(transform.DOScaleY(transform.localScale.y * 0.6f, 0.25f))
            .Join(transform.DOScaleX(transform.localScale.x * 1.1f, 0.25f)).Join(transform.DOScaleZ(transform.localScale.z * 1.1f, 0.25f))
            .Append(transform.DOScaleY(transform.localScale.y * 1.2f, 0.2f)).Join(transform.DOScaleX(transform.localScale.x * 0.8f, 0.2f))
            .Join(transform.DOScaleZ(transform.localScale.z * 0.8f, 0.2f)).Append(transform.DOScale(new Vector3(1, 0.25f, 1), 0.25f));
    }
    
    public void OffCollider() => _cellCollider.enabled = false; 
    
    public void DestroyCell()
    {
        Destroy(gameObject, 0.8f);
        _currentTween.Kill();
        _currentTween = DOTween.Sequence().Append(transform.DOScale(transform.localScale * 1.2f, 0.2f))
            .Append(transform.DOScale(transform.localScale * 0f, 0.4f));
    }

    private void OnDestroy() {
        _currentTween.Kill();
    }
}