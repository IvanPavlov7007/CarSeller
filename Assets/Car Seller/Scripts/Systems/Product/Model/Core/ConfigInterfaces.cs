
// example: [CreateAssetMenu(fileName = "WheelBaseConfig", menuName = "Configs/Products/Wheel/Wheel Base Config")]
public interface IBaseConfig
{
}

//example: [CreateAssetMenu(fileName = "WheelVariantConfig", menuName = "Configs/Products/Wheel/Wheel Variant Config")]
public interface IVariantConfig
{
    IBaseConfig FallbackBase { get; }
    bool OverrideBase { get; }
    IConfigOverrides Overrides { get; }
}

public interface IConfigOverrides
{
}

public interface IRuntimeConfig
{
    
}