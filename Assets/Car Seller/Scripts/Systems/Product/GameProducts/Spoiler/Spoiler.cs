using System.Collections;
using UnityEngine;
public sealed class Spoiler : Product
{
    public SpoilerRuntimeConfig runtimeConfig;

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildSpoiler(this);
    }
}