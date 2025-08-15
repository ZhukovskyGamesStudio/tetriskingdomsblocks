using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopOffersConfig", menuName = "Scriptable Objects/ShopOffersConfig")]
public class ShopOffersConfig : ScriptableObject {
    public List<SpecialOfferData> SpecialOffers;
    public List<SpecialOfferData> BundleOffers;
    public List<ResourceOfferData> ResourceOffers;

    public int BuyPieceForCoinsCost = 500;
    public BuyPieceForCoinsOffer BuyPieceForCoinsOffer;
}

[Serializable]
public class SpecialOfferData {
    public float Price;
    [SerializedDictionary]
    public SerializedDictionary<ResourceType, int> Resources;

    public RealShopOffer Prefab;
}

[Serializable]
public class ResourceOfferData {
    public Sprite Icon;
    public float Price;
    public float ImageScale = 1;
    public ResourceType Resource;
    public int ResourceCount;
}

