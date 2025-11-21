using System.Collections;
using UnityEngine;

public sealed class Wheel : Product
{
    public WheelRuntimeConfig runtimeConfig;
    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildWheel(this);
    }
}