using System;
using UnityEngine;

public class MetaEntryPoint : MonoBehaviour {

    [SerializeField]
    private MetaFieldManager _metaFieldManager;
    private void Start() {
        _metaFieldManager.SetupGame();
    }
}
