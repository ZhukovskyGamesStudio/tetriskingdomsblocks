using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopOffersConfig", menuName = "Scriptable Objects/ShopOffersConfig")]
public class ShopOffersConfig : ScriptableObject {
    public List<SpecialOfferData> SpecialOffers;
    public List<ResourceOfferData> ResourceOffers;
}

[Serializable]
public class SpecialOfferData {
    public float Price;
    public Sprite Icon;
    [SerializedDictionary]
    public SerializedDictionary<ResourceType, int> Resources;
}

[Serializable]
public class ResourceOfferData {
    public Sprite Icon;
    public float Price;
    public ResourceType Resource;
    public int ResourceCount;
}

