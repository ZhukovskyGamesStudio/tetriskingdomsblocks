using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class NextPiecesView : MonoBehaviour, IResetable {
    public static NextPiecesView Instance;

    [SerializeField]
    private List<Transform> _piecesContainers;

    [SerializeField]
    private float _piecesScale = 0.4f;

    [SerializeField]
    private float _creatingInterval = 0.2f, _creatingDelay = 0.1f;

    [SerializeField]
    private ParticleSystem _createParticleSystem;
    [SerializeField]
    private List<AudioSource> _appearAudioSource;

    [SerializeField]
    private List<ParticleSystem> _spawnParticles;
    
    private CancellationTokenSource _cts;

    private void Awake() {
        Instance = this;
    }

    public void SetData(PieceData nextPiece) {
        DestroyPieces();
        if (_piecesContainers.Count == 0) {
            Debug.LogWarning("NextPiecesView: No containers available for the pieces.");
            return;
        }

        SetData(new List<PieceData>() { nextPiece });
    }

    public void SetData(List<PieceData> nextPieces) {
        DestroyPieces();
        TryCancelCreatingTask();
        _cts = new CancellationTokenSource();
      
        CreatePiecesAsync(nextPieces, _cts.Token).Forget();
    }

    private async UniTask CreatePiecesAsync(List<PieceData> nextPieces, CancellationToken token) {
      
        
        
        for (int i = 0; i < nextPieces.Count; i++) {
            token.ThrowIfCancellationRequested();

            if (i >= _piecesContainers.Count) {
                Debug.LogWarning("NextPiecesView: Not enough containers for the pieces.");
                break;
            }

            Transform container = _piecesContainers[i];
            PieceView go = Instantiate(PiecesViewTable.Instance.PieceViewPrefab, container);
            go.SetData(nextPieces[i], _piecesScale);
            go.AppearAsync().Forget();
            _spawnParticles[i].gameObject.SetActive(true);
            _spawnParticles[i].Play();
           
            //await UniTask.WaitWhile(()=>    _appearAudioSource[i].isPlaying, cancellationToken: token);
        }
        _createParticleSystem.Play();
        _appearAudioSource[0].Play();
        await UniTask.Delay(TimeSpan.FromSeconds(_creatingInterval), cancellationToken: token);
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
        for (int i = 0; i < _piecesContainers.Count; i++)
        {
            DestroyCellsAnimation(_piecesContainers[i]);
            if (_piecesContainers[i].childCount != 0)
            {
                _spawnParticles[i].gameObject.SetActive(true);
                _spawnParticles[i].Play();
            }
        }

        
    }

    public void DestroyAdditionalPiece()
    {
        if (GameFieldManager.Instance._additionalPiecePrefab != null)
                {
                    DestroyCellsAnimation(GameFieldManager.Instance._additionalPieceContainer);
                    if (GameFieldManager.Instance._additionalPieceContainer.childCount != 0)
                    {
                        _spawnParticles[_piecesContainers.Count].gameObject.SetActive(true);
                        _spawnParticles[_piecesContainers.Count].Play();
                    }
                }
    }

    private void DestroyCellsAnimation(Transform cellsContainer)
    {
      //  float animationMultiplayer = ConfigsManager.Instance.DragConfig.DestroyPieceAnimationMultiplayer;

    //    DOTween.Sequence()
          //  .Append(cellsContainer.DOScale(cellsContainer.localScale * 1.1f, 0.2f * animationMultiplayer))
         //   .Append(cellsContainer.DOScale(0, 0.2f * animationMultiplayer)).OnComplete(() =>
         //  {
                foreach (Transform child in cellsContainer)
                {
                    Destroy(child.gameObject);
                }
         //  });

        //particles
    }
    
    public void Reset() {
        DestroyPieces();
    }
}