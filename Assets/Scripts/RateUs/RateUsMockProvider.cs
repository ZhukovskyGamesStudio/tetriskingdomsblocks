using Cysharp.Threading.Tasks;
using UnityEngine;

public class RateUsMockProvider : IRateUsProvider {
    public async UniTask Show() {
        await UniTask.WaitForEndOfFrame();
        Debug.Log("Rate us shown!");
    }
}