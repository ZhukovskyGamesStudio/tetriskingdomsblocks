using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class NextPiecesView : MonoBehaviour, IResetable {
    public static NextPiecesView Instance;

    [field: SerializeField]
    public List<Transform> _piecesContainers { get; private set; }

    [SerializeField]
    private float _piecesScale = 0.4f;

    [SerializeField]
    private float _creatingInterval = 0.2f, _creatingDelay = 0.1f;

    [SerializeField]
    private ParticleSystem _createParticleSystem;

    [SerializeField]
    private List<ParticleSystem> _spawnParticles;

    [SerializeField]
    private GameAudio _gameAudio;

    [SerializeField]
    private MeshRenderer _tinyPortalContainer;

    [SerializeField]
    private Collider _tinyPortalCollider;
    
    [SerializeField]
    private Material _tinyPortalActive, _tinyPortalInactive, _tinyPortalSelected;
    
    private CancellationTokenSource _cts;
    private Material _cur;
    private bool _isEnabled = true;
    private void Awake() {
        Instance = this;
    }

    public void SetTinyPortalActive(bool isActive) {
        _tinyPortalContainer.material = isActive ? _tinyPortalActive : _tinyPortalInactive;
        _tinyPortalCollider.enabled = isActive;
        _isEnabled = isActive;
    }

    private void Update() {
        if (_isEnabled) {
            bool isSelected = DragManager.IsDraggingPiece && GameFieldManager.Instance.AdditionalPieceContainerUnderPiece();
            _tinyPortalContainer.material = isSelected ? _tinyPortalSelected : _tinyPortalActive;
        }
    }

    public void SetData(List<PieceData> nextPieces) {
        DestroyPieces();
        TryCancelCreatingTask();
        _cts = new CancellationTokenSource();

        CreatePiecesAsync(nextPieces, _cts.Token, _piecesContainers).Forget();
    }

    public async UniTask<PieceView> CreateDynamitePieceView(Vector3 pos) {
        PieceData dynamiteCellInfo = PieceUtils.GetExactPiece(ConfigsManager.Instance.BoostersConfig.DinamyteCellInfo);
        TryCancelCreatingTask();
        _cts = new CancellationTokenSource();
        GameObject container = new GameObject("Dynamite Container");
        container.transform.position = pos;
        PieceView pieceView = await CreatePiecesAsync(new List<PieceData>() { dynamiteCellInfo }, _cts.Token,
            new List<Transform>() { container.transform }, true);
        pieceView.transform.SetParent(null);
        Destroy(container);
        return pieceView;
    }

    private async UniTask<PieceView> CreatePiecesAsync(List<PieceData> nextPieces, CancellationToken token, List<Transform> containers,
        bool isInstant = false) {
        PieceView pieceView = null;
        List<UniTask> appearTasks = new List<UniTask>();
        for (int i = 0; i < nextPieces.Count; i++) {
            token.ThrowIfCancellationRequested();

            if (i >= _piecesContainers.Count) {
                Debug.LogWarning("NextPiecesView: Not enough containers for the pieces.");
                break;
            }

            PieceView go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, containers[i]);
            pieceView = go;
            go.SetData(nextPieces[i], _piecesScale);
            if (isInstant) {
                go.AppearInstant();
            } else {
                appearTasks.Add(go.AppearAsync());
            }

            _spawnParticles[i].gameObject.SetActive(true);
            _spawnParticles[i].Play();
        }

        _createParticleSystem.Play();
        _gameAudio.PlayNextSound(_gameAudio.PiecesAppear);
        await UniTask.WhenAll(appearTasks);

        return pieceView;
    }

    private void TryCancelCreatingTask() {
        if (_cts == null || _cts.IsCancellationRequested) {
            return;
        }

        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    public void DestroyPieces() {
        for (int i = 0; i < _piecesContainers.Count; i++) {
            DestroyCellsAnimation(_piecesContainers[i]);
            if (_piecesContainers[i].childCount != 0) {
                _spawnParticles[i].gameObject.SetActive(true);
                _spawnParticles[i].Play();
            }
        }
    }

    public void DestroyAdditionalPiece() {
        if (GameFieldManager.Instance.AdditionalPiecePrefab != null) {
            DestroyCellsAnimation(GameFieldManager.Instance.AdditionalPieceContainer);
            if (GameFieldManager.Instance.AdditionalPieceContainer.childCount != 0) {
                _spawnParticles[_piecesContainers.Count].gameObject.SetActive(true);
                _spawnParticles[_piecesContainers.Count].Play();
            }
        }
    }

    private void DestroyCellsAnimation(Transform cellsContainer) {
        //  float animationMultiplayer = ConfigsManager.Instance.DragConfig.DestroyPieceAnimationMultiplayer;

        //    DOTween.Sequence()
        //  .Append(cellsContainer.DOScale(cellsContainer.localScale * 1.1f, 0.2f * animationMultiplayer))
        //   .Append(cellsContainer.DOScale(0, 0.2f * animationMultiplayer)).OnComplete(() =>
        //  {
        foreach (Transform child in cellsContainer) {
            Destroy(child.gameObject);
        }
        //  });

        //particles
    }

    public void Reset() {
        DestroyPieces();
    }
}