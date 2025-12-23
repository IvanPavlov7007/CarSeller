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
        return IconBuilderManager.Instance.BuildWheel(wheel);
    }

    public static Sprite BuildSpoilerSprite(Spoiler spoiler)
    {
        return IconBuilderManager.Instance.BuildSpoiler(spoiler);
    }

    public static Sprite BuildEngineSprite(Engine engine)
    {
        return IconBuilderManager.Instance.BuildEngine(engine);
    }
}