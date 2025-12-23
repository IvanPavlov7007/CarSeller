using System.Collections;
using UnityEngine;
public static class IconBuilderHelper
{
    public static Sprite BuildCarSprite(Car car)
    {
        return IconBuilderManager.Instance.BuildCar(car);
    }

    public static Sprite BuildCarFrameSprite(CarFrame carFrame)
    {
        return IconBuilderManager.Instance.BuildCarFrame(carFrame);
    }

    public static Sprite BuildWheelSprite(Wheel wheel)
    {
        return wheel.runtimeConfig.FrontSideViewSprite;
    }

    public static Sprite BuildSpoilerSprite(Spoiler spoiler)
    {
        return spoiler.runtimeConfig.Sprite;
    }

    public static Sprite BuildEngineSprite(Engine engine)
    {
        return engine.runtimeConfig.Sprite;
    }
}