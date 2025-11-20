using System.Collections;
using UnityEngine;

public sealed class Wheel : Product
{
    WheelRuntimeConfig runtimeConfig;
    public override Sprite GetIcon()
    {
        return IconBuilder.BuildWheelSprite(runtimeConfig);
    }
}