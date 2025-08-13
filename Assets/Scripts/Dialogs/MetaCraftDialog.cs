using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MetaCraftDialog : DialogBase {
    [SerializeField]
    private MetaCraft _craftPrefab;

    [SerializeField]
    private Transform _craftsContainer;

    private Action<MetaCraftInfo> _craft;
    
    public override void SetData(object data) {
        Data dialogData = data as Data;
        _craft = dialogData.Craft;

        foreach (MetaCraftInfo craft in dialogData.Crafts) {
            MetaCraft newCraft = Instantiate(_craftPrefab, _craftsContainer);
            MetaCraftInfo craftInfo = craft;
            bool hasCell = MetaFieldManager.Instance.HasPieceInInventory(craftInfo.NeededCell);
            newCraft.SetData(craft, () => Craft(craftInfo), hasCell);
        }
    }

    private void Craft(MetaCraftInfo craftInfo) {
        _craft.Invoke(craftInfo);
        Hide().Forget();
    }

    [Serializable]
    public class Data {
        public List<MetaCraftInfo> Crafts;
        public Action<MetaCraftInfo> Craft;
    }
}