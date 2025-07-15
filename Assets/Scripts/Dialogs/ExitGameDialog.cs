using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExitGameDialog : DialogBase {
    private Action _clickYes;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickYes = dialogData.СlickYes;
    }

    public void ClickYes() {
        _clickYes?.Invoke();
        Hide().Forget();
    }

    public void ClickNo() {
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public Action СlickYes;
    }
}