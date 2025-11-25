using System.Collections;
using UnityEngine;
public sealed class Engine : Product
{
    public EngineRuntimeConfig runtimeConfig;

    public override string Name => runtimeConfig.Name;

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildEngine(this);
    }
}