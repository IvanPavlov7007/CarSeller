using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CarPartViewBuilder", menuName = "Configs/View/Car Part View Builder")]
public class CarPartViewBuilder : ScriptableObject, IProductViewBuilder<GameObject>, IProductViewInitializer<ProductView>
{
    public GameObject baseWheelPrefab;
    public GameObject baseSpoilerPrefab;
    public GameObject baseEnginePrefab;

    [ShowInInspector]
    public static string frameChildName = "body";
    [ShowInInspector]
    public static string windshieldChildName = "windshield";

    public GameObject BuildCar(Car car)
    {
        Debug.LogError("CarPartViewBuilder does not support building full Car views. " + car.UniqueName);
        return null;
    }

    //TODO make a better and faster implementation, maybe change the stored value from prefab to something else
    public GameObject BuildCarFrame(CarFrame carFrame)
    {
        var frameGO = Instantiate(carFrame.runtimeConfig.Prefab);
        frameGO.name = carFrame.Name;
        var windshieldSpriteRenderer = CommonTools.AllChildren(frameGO?.transform).
            First(item => item.name.Contains(windshieldChildName, StringComparison.OrdinalIgnoreCase))?.GetComponent<SpriteRenderer>();
        var frameSpriteRenderer = CommonTools.AllChildren(frameGO?.transform).
            First(item => item.name.Contains(frameChildName, StringComparison.OrdinalIgnoreCase))?.GetComponent<SpriteRenderer>();

        windshieldSpriteRenderer.color = carFrame.runtimeConfig.WindshieldColor;
        frameSpriteRenderer.color = carFrame.runtimeConfig.FrameColor;
        InitializeView(frameGO, carFrame);
        CollisionBuilder.InitializeCollision(frameSpriteRenderer);
        return frameGO;
    }

    public GameObject BuildEngine(Engine engine)
    {
        var engineGO = Instantiate(baseEnginePrefab);
        engineGO.name = engine.Name;
        var engineSpriteRenderer = engineGO.GetComponent<SpriteRenderer>();
        //TODO add engine sprite etc
        CollisionBuilder.InitializeCollision(engineSpriteRenderer);
        InitializeView(engineGO, engine);
        return engineGO;
    }

    public GameObject BuildSpoiler(Spoiler spoiler)
    {
        var spoilerGO = Instantiate(baseSpoilerPrefab);
        spoilerGO.name = spoiler.Name;
        var spoilerSpriteRenderer = spoilerGO.GetComponent<SpriteRenderer>();
        spoilerSpriteRenderer.color = spoiler.runtimeConfig.Color;
        spoilerSpriteRenderer.sprite = spoiler.runtimeConfig.Sprite;

        var slotLocation = G.Instance.LocationService.GetProductLocation(spoiler) as Car.CarPartLocation;
        var data = slotLocation.PartSlotRuntimeConfig.partSlotData;

        // Apply slot positioning data
        var spoilerTransform = spoilerGO.transform;
        spoilerTransform.localScale = data.LocalScale * spoiler.runtimeConfig.Size;
        spoilerTransform.localRotation = Quaternion.Euler(data.LocalRotation);
        spoilerTransform.localPosition = data.LocalPosition;


        InitializeView(spoilerGO, spoiler);
        return spoilerGO;
    }

    public GameObject BuildWheel(Wheel wheel)
    {
        var wheelGO = Instantiate(baseWheelPrefab);

        wheelGO.name = wheel.Name;

        var wheelSpriteRenderer = wheelGO.GetComponent<SpriteRenderer>();
        wheelSpriteRenderer.color = wheel.runtimeConfig.Color;

        var slotLocation = G.Instance.LocationService.GetProductLocation(wheel) as Car.CarPartLocation;
        var chosenSprite = slotLocation.PartSlotRuntimeConfig.partSlotData.facingBackwards ?
            wheel.runtimeConfig.BackSideViewSprite : wheel.runtimeConfig.FrontSideViewSprite;
        var data = slotLocation.PartSlotRuntimeConfig.partSlotData;

        // Apply slot positioning data
        var wheelTransform = wheelGO.transform;
        wheelTransform.localRotation = Quaternion.Euler(data.LocalRotation);
        wheelTransform.localPosition = data.LocalPosition;
        wheelTransform.localScale = data.LocalScale * wheel.runtimeConfig.SideViewSize;

        wheelSpriteRenderer.sprite = chosenSprite;

        CollisionBuilder.InitializeCollision(wheelSpriteRenderer);
        InitializeView(wheelGO, wheel);
        return wheelGO;
    }

    public ProductView InitializeView(GameObject gameObject, Product product)
    {
        var controller = gameObject.AddComponent<ProductView>();
        controller.Initialize(product, G.Instance.LocationService.GetProductLocation(product));
        return controller;
    }
}

public static class CarPartViewPlacementHelper
{
    public static GameObject BuildCarPartAtPosition(Car.CarPartLocation carPartLocation, Transform parentCarViewTransform, CarPartViewBuilder carPartViewBuilder)
    {
        Debug.Assert(carPartLocation.Product != null, "Car part location has no product attached: " + carPartLocation.PartSlotRuntimeConfig.SlotType);

        var slotData = carPartLocation.PartSlotRuntimeConfig.partSlotData;

        var part = carPartLocation.Product.GetRepresentation(carPartViewBuilder);
        if (part != null)
        {
            part.transform.SetParent(parentCarViewTransform);
            part.transform.localPosition = slotData.LocalPosition;
            part.transform.localRotation = Quaternion.Euler(slotData.LocalRotation);
            part.transform.localScale = slotData.LocalScale;
        }

        return part;
    }
}