using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
[Serializable]
public abstract class PartSlotRuntimeConfig : IPartSlot
{
    public PartSlotData partSlotData;
    public abstract PartSlotType SlotType { get; }
    public abstract bool TryAccept(Product product);
    public abstract void Detach();

    
    
}
[Serializable]
public struct PartSlotData
{
    public bool Required;
    public bool Hidden; // not shown, e g engine
    [HideIf("Hidden")]
    public Vector2 LocalPosition;
    [HideIf("Hidden")]
    public Vector3 LocalRotation;
    [HideIf("Hidden")]
    public Vector3 LocalScale;
    [HideIf("Hidden")]
    public bool facingBackwards;

}

[Serializable]
public class EngineSlotRuntimeConfig : PartSlotRuntimeConfig
{
    public override PartSlotType SlotType => PartSlotType.Engine;
    public EngineRuntimeConfig engineConfig;

    public override bool TryAccept(Product product)
    {
        Engine engine = product as Engine;
        if (engine != null)
        {
            engineConfig = engine.runtimeConfig;
            return true;
        }
        return false;
    }

    public override void Detach()
    {
        engineConfig = null;
    }
}
[Serializable]
public class WheelSlotRuntimeConfig : PartSlotRuntimeConfig
{
    public override PartSlotType SlotType => PartSlotType.Wheels;
    public WheelRuntimeConfig wheelConfig;
    public override bool TryAccept(Product product)
    {
        Wheel wheel = product as Wheel;
        if (wheel != null)
        {
            wheelConfig = wheel.runtimeConfig;
            return true;
        }
        return false;
    }
    public override void Detach()
    {
        wheelConfig = null;
    }
}

[Serializable]
public class SpoilerSlotRuntimeConfig : PartSlotRuntimeConfig
{
    public override PartSlotType SlotType => PartSlotType.Spoiler;
    public SpoilerRuntimeConfig spoilerConfig;
    public override bool TryAccept(Product product)
    {
        Spoiler spoiler = product as Spoiler;
        if (spoiler != null)
        {
            spoilerConfig = spoiler.runtimeConfig;
            return true;
        }
        return false;
    }
    public override void Detach()
    {
        spoilerConfig = null;
    }
}