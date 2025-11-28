
// example: [CreateAssetMenu(fileName = "WheelBaseConfig", menuName = "Configs/Products/Wheel/Wheel Base Config")]
using Sirenix.OdinInspector;

public interface IBaseConfig
{
}

//example: [CreateAssetMenu(fileName = "WheelVariantConfig", menuName = "Configs/Products/Wheel/Wheel Variant Config")]
public interface IVariantConfig
{
    IBaseConfig FallbackBase { get; }
    bool ForceFallback { get; }
    IConfigOverrides Overrides { get; }
}



public interface IConfigOverrides
{
}

public interface IRuntimeConfig
{
    
}