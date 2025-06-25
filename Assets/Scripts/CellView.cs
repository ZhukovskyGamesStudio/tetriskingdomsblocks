using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class CellView : MonoBehaviour {
    [field: SerializeField]
    public CellType CellType;

    [SerializeField]
    private GameObject _objectsContainer;

    [SerializeField]
    private Collider _cellCollider;

    [SerializeField]
    private List<GameObject> _selectOneList;

    private Tween _currentTween;
    public Guid Seed { get; private set; } = Guid.NewGuid();

    public void SetSeed(Guid seed) {
        Seed = seed;
        if (_objectsContainer) {
            RandomRotateObjects(Seed);
        }

        if (_selectOneList != null && _selectOneList.Count > 0) {
            EnableRandomFromList(_selectOneList);
        }
    
    }

    private void EnableRandomFromList(List<GameObject> list) {
        var rnd = Random.Range(0, list.Count);
        for (int i = 0; i < list.Count; i++) {
            list[i].SetActive(i == rnd);
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