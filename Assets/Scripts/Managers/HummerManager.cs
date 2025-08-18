using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HummerManager : MonoBehaviour {
    public static HummerManager Instance;
    private Tween _hummerTween;
    private Sequence _hummerSequence;

    [SerializeField]
    private Transform _hummerContainer;
    
    [SerializeField]
    private float _appearDuration = 0.4f, _hitDuration = 0.4f, _hideDuration = 0.2f;

    [SerializeField]
    private float _hammerZInitAngle = 30f, _hammerYRotateAngle = 45f;

    [SerializeField]
    private Vector3 _hammerCellShift = new Vector3(0.25f, -0.5f, -0.25f);

    private void Awake() {
        Instance = this;
    }

    public async UniTask HummerDestroyPieceAnimation(Vector3 cellPosition) {
        _hummerSequence.Kill();
        _hummerSequence = DOTween.Sequence();
        var finPos = new Vector3(cellPosition.x, cellPosition.y, cellPosition.z) + _hammerCellShift;
        _hummerContainer.transform.localScale = Vector3.zero;
        _hummerContainer.transform.localRotation = Quaternion.Euler(0, _hammerYRotateAngle, _hammerZInitAngle);
        _hummerContainer.transform.position = finPos;
        _hummerSequence.Append(_hummerContainer.transform.DOScale(Vector3.one, _appearDuration))
            .Append(_hummerContainer.transform.DORotate(new Vector3(0, _hammerYRotateAngle, 90f), _hitDuration / 2))
            .Append(_hummerContainer.transform.DORotate(new Vector3(0, _hammerYRotateAngle, _hammerZInitAngle), _hitDuration / 2))
            .Append(_hummerContainer.transform.DOScale(Vector3.zero, _hideDuration));
        await UniTask.Delay(TimeSpan.FromSeconds(_appearDuration + _hitDuration / 2));
    }
}