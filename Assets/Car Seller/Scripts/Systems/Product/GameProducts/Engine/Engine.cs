using System.Collections;
using UnityEngine;
public sealed class Engine : Product
{
    public EngineRuntimeConfig runtimeConfig;
    public override Sprite GetIcon()
    {
        return IconBuilder.BuildEngineSprite(runtimeConfig);
    }
}