using System;
using System.Collections.Generic;
using UnityEngine;

public class MetaCraftDialog : DialogBase {
    [SerializeField]
    private MetaCraft _craftPrefab;

    [SerializeField]
    private Transform _craftsContainer;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;

        foreach (MetaCraftInfo craft in dialogData.Crafts) {
            MetaCraft newCraft = Instantiate(_craftPrefab, _craftsContainer);
            MetaCraftInfo craftInfo = craft;
            newCraft.SetData(craft, () => dialogData.Craft(craftInfo));
        }
    }

    [Serializable]
    public class Data {
        public List<MetaCraftInfo> Crafts;
        public Action<MetaCraftInfo> Craft;
    }
}