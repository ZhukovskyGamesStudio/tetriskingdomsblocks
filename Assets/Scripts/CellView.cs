using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using ScriptableObjects.Configs;
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
    
    //TODO move into config
    private float _upgradeTime = 0.4f;
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
            if (i == rnd) {
                list[i].SetActive(true);
                while (list[i].transform.childCount>0) {
                    list[i].transform.GetChild(0).SetParent(transform, true);
                }
                list[i].transform.SetSiblingIndex(0);
            } else {
                Destroy(list[i]);
            }
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


    [ItemCanBeNull]
    public List<Transform> Children => GetComponentsInChildren<Transform>( false).OrderBy(c=>c.GetSiblingIndex()).ToList();

    public Sequence DropWithDecorSequence(DragConfig cnfg,  float finY) {
        var animSpeedMultiplayer = cnfg.AfterDropPieceAnimationMultiplayer;
        var seq = DOTween.Sequence();

        var children = Children;
        children.Remove(transform);
        var invertedCurve = InvertCurve(cnfg.DropPieceAnimationCurve);
        for (int index = 0; index < children.Count; index++) {
            Transform tr = children[index];
            var cellSeq = DOTween.Sequence();
            cellSeq.AppendInterval(cnfg._delayBetweenDecorDrop * animSpeedMultiplayer * index);
            cellSeq.Append(tr.transform.DOMoveY(endValue: finY, cnfg._dropLength * animSpeedMultiplayer).SetEase(invertedCurve));
            seq.Join(cellSeq);
        }

        return seq;
    }
  
   

    public void UpgradeStart() {
        DOTween.Sequence().Append(transform.DOScale(transform.localScale * 0f, _upgradeTime / 2));
    }

    public void UpgradeEnd(DragConfig dragConfig,float finY) {
        var finScale = transform.localScale;
        transform.localScale = Vector3.zero;
        DOTween.Sequence().AppendInterval(_upgradeTime / 2).Append(transform.DOScale(finScale, _upgradeTime / 2));
    }
    
    public static AnimationCurve InvertCurve(AnimationCurve original)
    {
        var keys = original.keys;
        for (int i = 0; i < keys.Length; i++)
        {
            keys[i].value = 1f - keys[i].value; // инверсия относительно 1
            keys[i].inTangent = -keys[i].inTangent;
            keys[i].outTangent = -keys[i].outTangent;
        }
        return new AnimationCurve(keys);
    }
    
    
    private void OnDestroy() {
        _currentTween.Kill();
    }
}