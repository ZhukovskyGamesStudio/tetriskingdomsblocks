using System.Collections.Generic;
using Abstract;
using UnityEngine;

public class MadPixelDDOL : PreloadableSingleton<MadPixelDDOL> {
    [SerializeField]
    private List<GameObject> _managers;

    public override int InitPriority => -100;
    protected override bool IsDontDestroyOnLoad => false;

    protected override void OnFirstInit() {
        base.OnFirstInit();
#if MADPIXEL
       ActivateManagers();
#else
        Destroy(gameObject);
        #endif
        
    }

    private void ActivateManagers() {
        foreach (GameObject manager in _managers) {
            manager.transform.SetParent(null);
            DontDestroyOnLoad(manager.gameObject);
            manager.SetActive(true);
        }
    }
}
