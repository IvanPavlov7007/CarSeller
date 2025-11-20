using System.Collections;
using UnityEngine;
public class PartSlotRuntimeConfig
{
    public PartSlotType partSlotType;

    public EngineRuntimeConfig engineRuntimeConfig;
    public WheelRuntimeConfig wheelRuntimeConfig;
    public SpoilerRuntimeConfig spoilerRuntimeConfig;
}

public enum PartSlotType { Engine, Wheels, Spoiler }