using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaManager : MonoBehaviour {
    private void Awake() {
        ChangeToLoading.TryChange();
    }

    public void Play() {
        SceneManager.LoadScene("GameScene");
    }
}