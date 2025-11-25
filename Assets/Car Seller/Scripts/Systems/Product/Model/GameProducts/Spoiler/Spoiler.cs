using System.Collections;
using UnityEngine;
public sealed class Spoiler : Product
{
    public override string Name => runtimeConfig.Name;

    public SpoilerRuntimeConfig runtimeConfig;

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildSpoiler(this);
    }
}