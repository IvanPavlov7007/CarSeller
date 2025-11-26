using System.Collections;
using UnityEngine;

public sealed class Wheel : Product
{
    public WheelRuntimeConfig runtimeConfig;

    public override string Name => runtimeConfig.Name;

    public Wheel(WheelRuntimeConfig runtimeConfig)
    {
        this.runtimeConfig = runtimeConfig;
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildWheel(this);
    }
}