using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LootboxDialog : DialogBase {
    [SerializeField]
    private GameObject _openState, _continueState;

    private PieceData _rewardingPiece;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;

        _rewardingPiece = dialogData.RewardingPiece;
        
        _openState.SetActive(true);
        _continueState.SetActive(false);
    }

    public void ClickOpen() {
        _openState.SetActive(false);
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