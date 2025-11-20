using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class PartSlotVariantConfig
{
    [LabelText("Part Slot Type")]
    [OnValueChanged("HandleChange")]
    public PartSlotType partSlotType;

    [LabelText("Assign This Variant")]
    [OnValueChanged("HandleChange")]
    public bool assignThisVariant = true;

    // Base configs (shown only when assignThisVariant == true and matching partSlotType)
    [ShowIf("@assignThisVariant && partSlotType == PartSlotType.Wheels")]
    [BoxGroup("Wheel Configuration")]
    public WheelBaseConfig wheelBaseConfig;

    [ShowIf("@assignThisVariant && partSlotType == PartSlotType.Engine")]
    [BoxGroup("Engine Configuration")]
    public EngineBaseConfig engineBaseConfig;

    [ShowIf("@assignThisVariant && partSlotType == PartSlotType.Spoiler")]
    [BoxGroup("Spoiler Configuration")]
    public SpoilerBaseConfig spoilerBaseConfig;

    // Variant configs (paired visibility with base config)
    [ShowIf("@assignThisVariant && partSlotType == PartSlotType.Wheels")]
    [BoxGroup("Wheel Configuration")]
    public WheelVariantConfig wheelVariantConfig;

    [ShowIf("@assignThisVariant && partSlotType == PartSlotType.Engine")]
    [BoxGroup("Engine Configuration")]
    public EngineVariantConfig engineVariantConfig;

    [ShowIf("@assignThisVariant && partSlotType == PartSlotType.Spoiler")]
    [BoxGroup("Spoiler Configuration")]
    public SpoilerVariantConfig spoilerVariantConfig;

#if UNITY_EDITOR
    // Optional: Clean up unrelated config references when toggling
    private void HandleChange()
    {
        if (!assignThisVariant)
        {
            wheelBaseConfig = null;
            engineBaseConfig = null;
            spoilerBaseConfig = null;
            wheelVariantConfig = null;
            engineVariantConfig = null;
            spoilerVariantConfig = null;
            return;
        }

        switch (partSlotType)
        {
            case PartSlotType.Wheels:
                engineBaseConfig = null;
                spoilerBaseConfig = null;
                engineVariantConfig = null;
                spoilerVariantConfig = null;
                break;
            case PartSlotType.Engine:
                wheelBaseConfig = null;
                spoilerBaseConfig = null;
                wheelVariantConfig = null;
                spoilerVariantConfig = null;
                break;
            case PartSlotType.Spoiler:
                wheelBaseConfig = null;
                engineBaseConfig = null;
                wheelVariantConfig = null;
                engineVariantConfig = null;
                break;
        }
    }
#endif
}