using NUnit.Framework;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CarRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public float BasePrice;
    public float Speed;
    public float Acceleration;
    public Sprite TopView;
    public Sprite SideView;
    public CarFrameRuntimeConfig CarFrameRuntimeConfig;
    public List<PartSlotRuntimeConfig> SlotConfigs;

    public CarColor Color;
    public CarType Type;
    public CarRarity Rarity;
}