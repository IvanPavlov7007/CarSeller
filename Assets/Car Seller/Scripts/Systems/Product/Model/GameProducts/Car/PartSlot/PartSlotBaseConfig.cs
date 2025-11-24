using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
[System.Serializable]
public abstract class PartSlotBaseConfig
{
    public abstract PartSlotType SlotType { get; }

    public bool Hidden; // not shown, e g engine
    public Vector2 LocalPosition;
    public Vector3 LocalRotation;
    public Vector3 LocalScale;
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