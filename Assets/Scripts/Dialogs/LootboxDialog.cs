using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class LootboxDialog : DialogBase {
    private static readonly int Open = Animator.StringToHash("open");

    [SerializeField]
    private GameObject _openState, _continueState;

    [SerializeField]
    private Animator _chestAnimator;

    [SerializeField]
    private Transform _pieceContainer;

    [Header("Animation parameters")]
    [SerializeField]
    private Vector3 _finalRotation;

    [SerializeField]
    private float _fromScale, _toScale, _startYPos, _addedYPos, _appearDelay, _appearDuration, _rotationSpeed;

    [SerializeField]
    private Transform _particles;

    private PieceData _rewardingPiece;
    private PieceView _piece;
    private CancellationTokenSource _openCts;
    private bool _continueClicked;
    public override void SetData(object data) {
        Data dialogData = data as Data;

        _rewardingPiece = dialogData.RewardingPiece;
        _openCts = new CancellationTokenSource();
        _openState.SetActive(true);
        _continueState.SetActive(false);
    }

    public override UniTask Show(Action onClose) {
        AppearAndIdleSound(_openCts.Token).Forget();
        return base.Show(onClose);
    }

    private async UniTask AppearAndIdleSound(CancellationToken token) {
        GameAudio.Instance.PlayNextSoundWithDelay(GameAudio.Instance.LootboxAppear, 0f, token).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
        /* while (true) {
             GameAudio.Instance.PlayNextSoundWithDelay(GameAudio.Instance.LootboxIdle, 0f,token).Forget();
             await UniTask.Delay(TimeSpan.FromSeconds(5),cancellationToken:token);
         }*/
    }

    public void ClickOpen() {
        _openCts?.Cancel();
        // GameAudio.Instance.ForceStop(GameAudio.Instance.LootboxIdle);
        _chestAnimator.SetTrigger(Open);
        _openState.SetActive(false);
        GameAudio.Instance.PlayNextSoundWithDelay(GameAudio.Instance.LootboxOpen, 0f, this.GetCancellationTokenOnDestroy()).Forget();
        WaitForOpen().Forget();
        var res = CreatePiece(_rewardingPiece);
        AppearPieceAnim().Forget();
    }

    private async UniTask WaitForOpen() {
        await UniTask.WaitUntil(() => _chestAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        SetContinueState();
    }

    private PieceView CreatePiece(PieceData nextPiece) {
        _piece = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, _pieceContainer);
        _piece.SetData(nextPiece);
        _piece.enabled = false;
        MeshRenderer[] renderers = _piece.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in renderers) {
            meshRenderer.gameObject.layer = LayerMask.NameToLayer("Dialogs3d");
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
        _particles.gameObject.SetActive(true);
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
        if (_continueClicked) {
            return;
        }

        _particles.gameObject.SetActive(false);
        _continueClicked = true;

        Close().Forget();
    }

    private async UniTask Close() {
        await MetaFieldManager.Instance.AddPieceToInventory(_rewardingPiece);
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public PieceData RewardingPiece;
    }
}