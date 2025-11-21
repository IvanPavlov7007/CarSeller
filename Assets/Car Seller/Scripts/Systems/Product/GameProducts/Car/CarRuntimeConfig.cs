using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarRuntimeConfig
{
    public string Name;
    public CarFrameRuntimeConfig carFrameRuntimeConfig;
    public List<PartSlotRuntimeConfig> slotConfigs;
}