using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopOffersConfig", menuName = "Scriptable Objects/ShopOffersConfig")]
public class ShopOffersConfig : ScriptableObject {
    public List<OffersGroupData> OffersGroups;
}

[Serializable]
public class OfferData {
    public string Title;
    public string Id;
    public float Price;
    [SerializedDictionary]
    public SerializedDictionary<ResourceType, int> Resources;
}

[Serializable]
public class OffersGroupData {
    public string Title;
    public OfferData[] Offers;
}

