using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ExitGameDialog : DialogBase {
    private Action _clickExit;

    public override void SetData(object data) {
        Data dialogData = data as Data;

        _clickExit = dialogData.ClickExit;
    }

    public void ClickExit() {
        _clickExit.Invoke();
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public Action ClickExit;
    }
}