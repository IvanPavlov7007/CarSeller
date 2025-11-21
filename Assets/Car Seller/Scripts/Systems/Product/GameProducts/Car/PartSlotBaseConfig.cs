using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
[System.Serializable]
public abstract class PartSlotBaseConfig : ScriptableObject
{
    public abstract PartSlotType SlotType { get; }
}

public class EngineSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;
    public EngineBaseConfig engineBaseConfig;
}

public class WheelSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;
    public WheelBaseConfig wheelBaseConfig;
}

public class SpoilerSlotBaseConfig : PartSlotBaseConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;
    public SpoilerBaseConfig spoilerBaseConfig;
}