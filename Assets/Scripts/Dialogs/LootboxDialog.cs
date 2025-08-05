using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LootboxDialog : DialogBase {
    private static readonly int Open = Animator.StringToHash("open");

    [SerializeField]
    private GameObject _openState, _continueState;

    [SerializeField]
    private Animator _chestAnimation;

    private PieceData _rewardingPiece;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _rewardingPiece = dialogData.RewardingPiece;

        _openState.SetActive(true);
        _continueState.SetActive(false);
    }

    public void ClickOpen() {
        _openState.SetActive(false);
        _chestAnimation.SetTrigger(Open);
        WaitForOpen().Forget();
    }
    
    private async UniTask WaitForOpen() {
        await UniTask.WaitUntil(() => _chestAnimation.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        SetContinueState();
    }

    public void SetContinueState() {
        _continueState.SetActive(true);
    }

    public void ClickContinue() {
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public PieceData RewardingPiece;
    }
}