using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LootboxDialog : DialogBase {
    [SerializeField]
    private GameObject _openState, _continueState;

    [SerializeField]
    private Animation _chestAnimation;

    [SerializeField]
    private Animator _chestAnimator;

    [SerializeField]
    private AnimationClip _show, _idle, _open;

    [SerializeField]
    private float _fromScale, _toScale, _startYPos, _addedYPos, _appearDelay, _appearDuration, _rotationSpeed;

    [SerializeField]
    private Vector3 _finalRotation;

    private PieceData _rewardingPiece;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _rewardingPiece = dialogData.RewardingPiece;

        _openState.SetActive(true);
        _continueState.SetActive(false);
    }

    public override async UniTask Show(Action onClose) {
        await base.Show(onClose);
        _chestAnimation.Play(_show.name);
        _chestAnimation.PlayQueued(_idle.name);
    }

    public void ClickOpen() {
        _chestAnimator.SetTrigger("open");
        _openState.SetActive(false);
        _chestAnimation.Play(_open.name);
        WaitForOpen().Forget();
        var res = CreatePiece(_rewardingPiece);
        AppearPieceAnim().Forget();
    }

    private async UniTask WaitForOpen() {
        await UniTask.WaitUntil(() => _chestAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        //NextPiecesView.Instance.CreateDynamitePieceView(pos);
        //await UniTask.WaitWhile(() => _chestAnimation.isPlaying);
        SetContinueState();
    }

    private PieceView _piece;

    private PieceView CreatePiece(PieceData nextPiece) {
        _piece = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, _chestAnimation.transform.parent, true);
        _piece.SetData(nextPiece);
        _piece.enabled = false;
        _piece.transform.position = _chestAnimation.transform.position;
        var m = _piece.GetComponentsInChildren<MeshRenderer>();
        foreach (var VARIABLE in m) {
            VARIABLE.gameObject.layer = LayerMask.NameToLayer("Dialogs3d");
        }
        _piece.transform.position += Vector3.up * _startYPos;
        _piece.transform.localScale = Vector3.one * _fromScale;
        return _piece;
    }

    private async UniTask AppearPieceAnim() {
        await UniTask.Delay(TimeSpan.FromSeconds(_appearDelay));
        var finPos = _piece.transform.position + Vector3.up * _addedYPos;
        await DOTween.Sequence().Append(_piece.transform.DOScale(Vector3.one * _toScale, _appearDuration))
            .Join(_piece.transform.DOMove(finPos, _appearDuration)).Join(_piece.transform.DORotate(_finalRotation, _appearDuration))
            .AsyncWaitForCompletion();
        PieceIdleRotate().Forget();
    }

    private async UniTask PieceIdleRotate() {
        var token = this.GetCancellationTokenOnDestroy();
        while (true) {
            _piece.transform.Rotate(Vector3.up * _rotationSpeed * Time.deltaTime);
            await UniTask.WaitForEndOfFrame(token);
        }
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