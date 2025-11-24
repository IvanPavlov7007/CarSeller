using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public abstract class PartSlotVariantConfig
{
    public abstract PartSlotType SlotType { get; }
}

public class EngineSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;
    public EngineVariantConfig engineVariantConfig;
}

public class WheelSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;
    public WheelVariantConfig wheelVariantConfig;
}

public class SpoilerSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;
    public SpoilerVariantConfig spoilerVariantConfig;
}