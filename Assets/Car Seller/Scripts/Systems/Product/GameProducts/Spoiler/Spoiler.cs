using System.Collections;
using UnityEngine;
public sealed class Spoiler : Product
{
    SpoilerRuntimeConfig runtimeConfig;
    public override Sprite GetIcon()
    {
        return IconBuilder.BuildSpoilerSprite(runtimeConfig);
    }
}