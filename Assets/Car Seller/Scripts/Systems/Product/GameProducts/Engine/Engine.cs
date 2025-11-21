using System.Collections;
using UnityEngine;
public sealed class Engine : Product
{
    public EngineRuntimeConfig runtimeConfig;
    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildEngine(this);
    }
}