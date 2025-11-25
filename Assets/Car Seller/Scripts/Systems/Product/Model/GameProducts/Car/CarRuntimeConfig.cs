using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CarRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public CarFrameRuntimeConfig CarFrameRuntimeConfig;
    public List<PartSlotRuntimeConfig> SlotConfigs;
}