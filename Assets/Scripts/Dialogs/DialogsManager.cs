using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogsManager : MonoBehaviour {
    private Queue<DialogWithData> _dialogsQ = new Queue<DialogWithData>();

    [SerializeField]
    private List<DialogBase> _dialogsPrefabs = new List<DialogBase>();

    [SerializeField]
    private Transform _dialogsContainer;

    [SerializeField]
    private Canvas _dialogsCanvas;

    private DialogBase _currentDialog;
    
    public bool IsDialogActive => _currentDialog != null;

    private int _dialogsCanvasSortingOrder;

    public static DialogsManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
        _dialogsCanvasSortingOrder = _dialogsCanvas.sortingOrder;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowDialog(Type dialogType) {
        AddToQueue(new DialogWithData() {
            DialogType = dialogType,
            Data = null
        });
    }

    public void ShowDialogWithData(DialogWithData dialogWithData) {
        AddToQueue(dialogWithData);
    }

    public void CloseDialog(Type dialogType) {
        if (_currentDialog is null || _currentDialog.GetType() != dialogType) {
            return;
        }
        
        _currentDialog.Hide().Forget();
    }

    public void CloseAllDialogs() {
        _dialogsQ.Clear();
        _currentDialog?.Hide().Forget();
    }

    private void AddToQueue(DialogWithData dialogWithData) {
        if (_dialogsQ.Any(d => d.DialogType == dialogWithData.DialogType) && dialogWithData.DialogType != typeof(LootboxDialog)) {
            return;
        }

        if (_currentDialog != null && (_currentDialog.GetType() == dialogWithData.DialogType && dialogWithData.DialogType != typeof(LootboxDialog))) {
            return;
        }

        _dialogsQ.Enqueue(dialogWithData);
        TryShowFromQueue();
    }

    private void TryShowFromQueue() {
        if (_dialogsQ.Count == 0) {
            return;
        }

        if (_currentDialog != null) {
            return;
        }

        DialogWithData dialog = _dialogsQ.Dequeue();
        DialogBase prefab = _dialogsPrefabs.Find(d => d.GetComponent(dialog.DialogType) != null);
        _dialogsCanvas.sortingOrder = prefab.ForceOverlay ? 1000 : _dialogsCanvasSortingOrder;
        var dialogObj = Instantiate(prefab, _dialogsContainer);
        _currentDialog = dialogObj;
        dialogObj.SetData(dialog.Data);
        dialogObj.Show(() => {
            Destroy(_currentDialog.gameObject);
            _currentDialog = null;
            TryShowFromQueue();
        }).Forget();
    }
}

[Serializable]
public class DialogWithData {
    public Type DialogType;
    public object Data;
}