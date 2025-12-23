using Pixelplacement;
using UnityEngine;

public class IconBuilderManager : Singleton<IconBuilderManager>, IProductViewBuilder<Sprite>
{
    [Header("Rendering")]
    public Camera iconRenderCamera;
    public Transform buildPosition;

    [Tooltip("Icon texture width in pixels.")]
    [SerializeField] private int width = 256;

    [Tooltip("Icon texture height in pixels.")]
    [SerializeField] private int height = 256;

    [Tooltip("Transparent background color for the icon render.")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0);

    [Tooltip("Extra padding around the object (% of the largest bound dimension).")]
    [Range(0f, 0.5f)]
    [SerializeField] private float padding = 0.08f;

    [Tooltip("Pixels-per-unit used when creating the resulting Sprite.")]
    [SerializeField] private float spritePixelsPerUnit = 100f;

    [Tooltip("If true and the camera is orthographic, the object will be auto-scaled to fit the view with padding.")]
    [SerializeField] private bool fitToView = true;

    WarehouseProductGameObjectBuilder gameObjectBuilder => G.Instance.warehouseProductViewBuilder;

    RenderTexture _rt;

    public Sprite BuildCar(Car car)
    {
        var go = gameObjectBuilder.BuildCar(car);
        return BuildIconFromGameObject(go);
    }

    public Sprite BuildCarFrame(CarFrame carFrame)
    {
        // Warehouse builder doesn’t support CarFrame views (by design).
        Debug.LogWarning($"IconBuilderManager: Warehouse builder does not support CarFrame. {carFrame?.Name}");
        return null;
    }

    public Sprite BuildEngine(Engine engine)
    {
        var go = gameObjectBuilder.BuildEngine(engine);
        return BuildIconFromGameObject(go);
    }

    public Sprite BuildSpoiler(Spoiler spoiler)
    {
        var go = gameObjectBuilder.BuildSpoiler(spoiler);
        return BuildIconFromGameObject(go);
    }

    public Sprite BuildWheel(Wheel wheel)
    {
        var go = gameObjectBuilder.BuildWheel(wheel);
        return BuildIconFromGameObject(go);
    }

    // Core flow: stage -> frame -> render -> cleanup
    private Sprite BuildIconFromGameObject(GameObject go)
    {
        if (iconRenderCamera == null || buildPosition == null)
        {
            SafeDestroy(go);
            Debug.LogError("IconBuilderManager: iconRenderCamera and buildPosition must be assigned.");
            return null;
        }

        if (go == null)
        {
            Debug.LogError("IconBuilderManager: Source GameObject is null.");
            return null;
        }

        try
        {
            Stage(go);
            CenterOnBounds(go);

            if (fitToView && iconRenderCamera.orthographic)
            {
                FitOrthographic(go, iconRenderCamera, padding);
            }

            return RenderSprite();
        }
        finally
        {
            SafeDestroy(go);
        }
    }

    // Place as a child of buildPosition, reset local transform for predictable staging.
    private void Stage(GameObject go)
    {
        go.hideFlags = HideFlags.HideAndDontSave;
        var t = go.transform;
        t.SetParent(buildPosition, worldPositionStays: false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        // Keep the scale provided by the warehouse builder; we may adjust later if fitting is enabled.
    }

    // Center the staged object so that the rendered bounds are centered at buildPosition.
    private void CenterOnBounds(GameObject go)
    {
        if (!TryGetBounds(go, out var bounds)) return;
        var delta = buildPosition.position - bounds.center;
        go.transform.position += delta;
    }

    // If the camera is orthographic, scale uniformly so the object fits into the camera view with padding.
    private void FitOrthographic(GameObject go, Camera cam, float pad)
    {
        if (!TryGetBounds(go, out var bounds)) return;

        // View size in world units
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;

        // Desired max size considering padding
        float targetWidth = viewWidth * (1f - pad);
        float targetHeight = viewHeight * (1f - pad);

        // Current size
        float sizeX = Mathf.Max(bounds.size.x, 1e-4f);
        float sizeY = Mathf.Max(bounds.size.y, 1e-4f);

        float scaleX = targetWidth / sizeX;
        float scaleY = targetHeight / sizeY;

        float uniformScale = Mathf.Min(scaleX, scaleY);

        // Apply on root uniformly
        go.transform.localScale *= uniformScale;

        // Re-center after scaling (bounds.center changed)
        CenterOnBounds(go);
    }

    private bool TryGetBounds(GameObject go, out Bounds bounds)
    {
        bounds = new Bounds();
        var renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);
        bool hasAny = false;

        foreach (var r in renderers)
        {
            // Skip non-visible renderers
            if (!r.enabled) continue;

            if (!hasAny)
            {
                bounds = r.bounds;
                hasAny = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        return hasAny;
    }

    private RenderTexture GetOrCreateRT()
    {
        if (_rt != null && (_rt.width != width || _rt.height != height))
        {
            _rt.Release();
            SafeDestroy(_rt);
            _rt = null;
        }

        if (_rt == null)
        {
            _rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            _rt.Create();
        }
        return _rt;
    }

    private Sprite RenderSprite()
    {
        var cam = iconRenderCamera;
        var prevTarget = cam.targetTexture;
        var prevClearFlags = cam.clearFlags;
        var prevBg = cam.backgroundColor;

        var rt = GetOrCreateRT();
        cam.targetTexture = rt;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;

        // Render immediately
        cam.Render();

        // Read back
        RenderTexture activePrev = RenderTexture.active;
        RenderTexture.active = rt;

        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
        tex.Apply(false, false);

        // Cleanup camera state
        RenderTexture.active = activePrev;
        cam.targetTexture = prevTarget;
        cam.clearFlags = prevClearFlags;
        cam.backgroundColor = prevBg;

        // Create sprite
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), spritePixelsPerUnit);
        sprite.name = "IconSprite";

        return sprite;
    }

    private static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Object.DestroyImmediate(obj);
            return;
        }
#endif
        Object.Destroy(obj);
    }
}