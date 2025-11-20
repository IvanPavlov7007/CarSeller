using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
[System.Serializable]
public class PartSlotBaseConfig
{
    [LabelText("Part Slot Type")]
    [OnValueChanged("HandleChange")]
    public PartSlotType partSlotType;

    [ShowIf("@partSlotType == PartSlotType.Wheels")]
    public WheelBaseConfig wheelBaseConfig;
    [ShowIf("@partSlotType == PartSlotType.Engine")]
    public EngineBaseConfig engineBaseConfig;
    [ShowIf("@partSlotType == PartSlotType.Spoiler")]
    public SpoilerBaseConfig spoilerBaseConfig;

#if UNITY_EDITOR
    // Optional: Clean up unrelated config references when toggling
    private void HandleChange()
    {

        switch (partSlotType)
        {
            case PartSlotType.Wheels:
                engineBaseConfig = null;
                spoilerBaseConfig = null;
                break;
            case PartSlotType.Engine:
                wheelBaseConfig = null;
                spoilerBaseConfig = null;
                break;
            case PartSlotType.Spoiler:
                wheelBaseConfig = null;
                engineBaseConfig = null;
                break;
        }
    }
#endif
}

//public interface IVariantConfigProvider<TBase, TVariant> { }