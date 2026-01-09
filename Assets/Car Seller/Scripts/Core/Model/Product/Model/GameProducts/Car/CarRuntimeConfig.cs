using NUnit.Framework;
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
    public CarFrameRuntimeConfig CarFrameRuntimeConfig;
    public List<PartSlotRuntimeConfig> SlotConfigs;
}