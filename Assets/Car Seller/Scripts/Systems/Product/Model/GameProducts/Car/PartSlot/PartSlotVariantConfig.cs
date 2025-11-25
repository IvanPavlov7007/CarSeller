using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public abstract class PartSlotVariantConfig
{
    public abstract PartSlotType SlotType { get; }
    public abstract IVariantConfig VariantConfig { get; }

    public PartSlotData partSlotData;
}

public class EngineSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;

    public override IVariantConfig VariantConfig => engineVariantConfig;

    public EngineVariantConfig engineVariantConfig;
}

public class WheelSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;

    public override IVariantConfig VariantConfig => wheelVariantConfig;

    public WheelVariantConfig wheelVariantConfig;
}

public class SpoilerSlotVariantConfig : PartSlotVariantConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;

    public override IVariantConfig VariantConfig => spoilerVariantConfig;

    public SpoilerVariantConfig spoilerVariantConfig;
}