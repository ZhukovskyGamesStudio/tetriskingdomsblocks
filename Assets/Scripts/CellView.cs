using System;
using DG.Tweening;
using UnityEngine;

public class CellView : MonoBehaviour {
    [field: SerializeField]
    public CellType CellType;

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

    public void OffCollider() => _cellCollider.enabled = false;

    public void DestroyCell() {
        Destroy(gameObject, 0.8f);
        _currentTween.Kill();
        var animSpeedMultiplayer = ConfigsManager.Instance.DragConfig.DestroyPieceAnimationMultiplayer;
        _currentTween = DOTween.Sequence().Append(transform.DOScale(transform.localScale * 1.2f, 0.2f * animSpeedMultiplayer))
            .Append(transform.DOScale(transform.localScale * 0f, 0.4f * animSpeedMultiplayer));
    }

    private void OnDestroy() {
        _currentTween.Kill();
    }
}