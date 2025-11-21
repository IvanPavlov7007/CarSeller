using System.Collections;
using UnityEngine;

public class CarFrame : Product
{
    public CarFrameRuntimeConfig runtimeConfig;

    public override string Name => runtimeConfig.Name;

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildCarFrame(this);
    }
}