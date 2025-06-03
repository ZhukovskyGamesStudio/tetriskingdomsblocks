using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaManager : MonoBehaviour {
    
    public static MetaManager Instance { get; private set; }
    
    [field:SerializeField]
    public MainMetaConfig MainMetaConfig { get;private set; }
    
    private void Awake() {
        ChangeToLoading.TryChange();
        Instance = this;
    }

    public void Play() {
        SceneManager.LoadScene("GameScene");
    }

    public void BuyPiece() {
        DialogsManager.Instance.ShowDialog(typeof(BuyPieceDialog));
    }

    public void CollectAll() {
        DialogsManager.Instance.ShowDialog(typeof(CollectAllDialog));
    }
}