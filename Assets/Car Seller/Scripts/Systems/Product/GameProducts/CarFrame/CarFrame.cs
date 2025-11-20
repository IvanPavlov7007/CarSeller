using System.Collections;
using UnityEngine;

public class CarFrame : Product
{
    public CarFrameRuntimeConfig runtimeConfig;
    public override Sprite GetIcon()
    {
        return IconBuilder.BuildCarFrameSprite(runtimeConfig);
    }
}