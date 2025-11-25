using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
[System.Serializable]
public abstract class PartSlotBaseConfig
{
    public abstract PartSlotType SlotType { get; }
    public abstract IBaseConfig BaseConfig { get; }

    public PartSlotData partSlotData;
}

public class EngineSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;

    public override IBaseConfig BaseConfig => engineBaseConfig;

    public EngineBaseConfig engineBaseConfig;
}

public class WheelSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;
    public override IBaseConfig BaseConfig => wheelBaseConfig;
    public WheelBaseConfig wheelBaseConfig;
}

public class SpoilerSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;
    public override IBaseConfig BaseConfig => spoilerBaseConfig;
    public SpoilerBaseConfig spoilerBaseConfig;
}