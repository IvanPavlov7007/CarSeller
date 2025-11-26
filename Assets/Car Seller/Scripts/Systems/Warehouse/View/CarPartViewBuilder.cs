using Sirenix.OdinInspector;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

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

    #region collision
    private void initializeCollision(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return;

        var collider = sr.gameObject.GetComponent<Collider2D>();

        // 1) If collider exists: adjust based on type.
        if (collider != null)
        {
            switch (collider)
            {
                case PolygonCollider2D poly:
                    RegeneratePolygon(poly, sr);
                    return;

                case BoxCollider2D box:
                    AdjustBox(box, sr);
                    return;

                case CircleCollider2D circle:
                    AdjustCircle(circle, sr);
                    return;

                // Other collider types are ignored intentionally.
                default:
                    return;
            }
        }

        // 2) If none exists: add PolygonCollider2D and regenerate.
        var newPoly = sr.gameObject.AddComponent<PolygonCollider2D>();
        RegeneratePolygon(newPoly, sr);
    }

    private void RegeneratePolygon(PolygonCollider2D poly, SpriteRenderer sr)
    {
        if (poly == null || sr == null || sr.sprite == null) return;

        var sprite = sr.sprite;
        int shapeCount = sprite.GetPhysicsShapeCount();

        if (shapeCount == 0)
        {
            // Fallback: use sprite rectangular bounds.
            var b = sprite.bounds;
            Vector2 min = b.min;
            Vector2 max = b.max;
            poly.pathCount = 1;
            poly.SetPath(0, new[]
            {
                new Vector2(min.x, min.y),
                new Vector2(min.x, max.y),
                new Vector2(max.x, max.y),
                new Vector2(max.x, min.y)
            });
            return;
        }

        poly.pathCount = shapeCount;
        var points = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i < shapeCount; i++)
        {
            points.Clear();
            sprite.GetPhysicsShape(i, points);
            // Points are already in local sprite space; assign directly.
            poly.SetPath(i, points);
        }
    }

    private void AdjustBox(BoxCollider2D box, SpriteRenderer sr)
    {
        if (box == null || sr == null || sr.sprite == null) return;

        // Use sprite local bounds for size and offset.
        var b = sr.sprite.bounds;
        box.size = b.size;
        box.offset = b.center;
    }

    private void AdjustCircle(CircleCollider2D circle, SpriteRenderer sr)
    {
        if (circle == null || sr == null || sr.sprite == null) return;

        var b = sr.sprite.bounds;
        // Radius = half of max dimension.
        float radius = Mathf.Max(b.size.x, b.size.y) * 0.5f;
        circle.radius = radius;
        circle.offset = b.center;
    }
    #endregion

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
        initializeCollision(frameSpriteRenderer);
        return frameGO;
    }

    public GameObject BuildEngine(Engine engine)
    {
        var engineGO = Instantiate(baseEnginePrefab);
        engineGO.name = engine.Name;
        var engineSpriteRenderer = engineGO.GetComponent<SpriteRenderer>();
        //TODO add engine sprite etc
        initializeCollision(engineSpriteRenderer);
        InitializeView(engineGO, engine);
        return engineGO;
    }

    public GameObject BuildSpoiler(Spoiler spoiler)
    {
        var spoilerGO = Instantiate(baseSpoilerPrefab);
        spoilerGO.name = spoiler.Name;
        var spoilerSpriteRenderer = spoilerGO.GetComponent<SpriteRenderer>();
        spoilerSpriteRenderer.color = spoiler.runtimeConfig.Color;

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

        initializeCollision(wheelSpriteRenderer);
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
