using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogsManager : MonoBehaviour {
    private Queue<Type> _dialogsQ = new Queue<Type>();

    [SerializeField]
    private List<DialogBase> _dialogsPrefabs = new List<DialogBase>();

    [SerializeField]
    private Transform _dialogsContainer;

    private DialogBase _currentDialog;

    public static DialogsManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void ShowDialog(Type dialogType) {
        AddToQueue(dialogType);
    }

    private void AddToQueue(Type dialog) {
        if (_dialogsQ.Contains(dialog)) {
            return;
        }

        _dialogsQ.Enqueue(dialog);
        TryShowFromQueue();
    }

    private void TryShowFromQueue() {
        if (_dialogsQ.Count == 0) {
            return;
        }

        if (_currentDialog != null) {
            return;
        }

        Type dialog = _dialogsQ.Dequeue();
        var prefab = _dialogsPrefabs.Find(d => d.GetComponent(dialog) != null);
        var dialogObj = Instantiate(prefab, _dialogsContainer);
        _currentDialog = dialogObj;
        dialogObj.Show(() => {
            Destroy(_currentDialog.gameObject);
            _currentDialog = null;
            TryShowFromQueue();
        }).Forget();
    }
}