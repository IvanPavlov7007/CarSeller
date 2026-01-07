using System;
using UnityEngine;

public class CarShopOffersConfig : ScriptableObject
{
    public CarShopOfferEntry[] offerConfigs;
}

[Serializable]
public class CarShopOfferEntry
{
    public SimpleCarSpawnConfig carSpawnConfig;
}