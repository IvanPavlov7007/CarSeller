using System.Collections;
using UnityEngine;
using Pixelplacement;
using System.Collections.Generic;
using UnityEngine.UI; // for LayoutRebuilder

public class ContextMenuManager : Singleton<ContextMenuManager>
{
    public GameObject popUpMenuPrefab;

    [Header("Global Context Menu Settings")]
    [Tooltip("Padding from screen edges (in pixels).")]
    public float EdgePadding = 12f;
    [Tooltip("Gap between the target and the menu (in pixels).")]
    public float SideMargin = 12f;
    [Tooltip("Minimum size of the target rect (in pixels) to avoid covering tiny points.")]
    public float TargetClearance = 72f;
    [Tooltip("Max height of the menu as a fraction of screen height.")]
    [Range(0.3f, 0.95f)]
    public float MaxHeightRatio = 0.66f;
    [Tooltip("Lerp factor (0-1) when following camera/target/screen changes.")]
    [Range(0.05f, 1f)]
    public float FollowLerp = 0.25f;

    private List<PopUpContextMenu> activeMenus = new List<PopUpContextMenu>();

    public void closeMenu(PopUpContextMenu menu)
    {
        if (activeMenus.Contains(menu))
        {
            activeMenus.Remove(menu);
            BlockUIManager.Instance.Unblock(ContextMenuCanvas.Instance.Canvas);
            Destroy(menu.gameObject);
        }
    }

    public void CreateContextMenu(GameObject target, UIElement content)
    {
        GameObject panel = Instantiate(popUpMenuPrefab, ContextMenuCanvas.Instance.Canvas.transform);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();

        // Build content first so layout can size to contents
        RectTransform contentRT = getContentTransform(panel);
        SimpleUIBuilder.Instance.Build(content, contentRT);

        // Create menu controller
        var ctxMenu = createContextMenu(panel, target.transform, content.blockingInput);
        ctxMenu.Initialize(target.transform, contentRT);

        // Apply global settings to the menu
        ApplyGlobalSettings(ctxMenu);

        // First layout pass and initial placement
        UpdateContextMenu(ctxMenu, true);
    }

    private RectTransform getContentTransform(GameObject panel)
    {
        // Using a LayoutGroup child as Content (VerticalLayoutGroup present)
        return panel.GetComponentInChildren<LayoutGroup>().GetComponent<RectTransform>();
    }

    private enum Side { Right, Left, Top, Bottom }

    private PopUpContextMenu createContextMenu(GameObject panel, Transform target, bool blocking)
    {
        var ctxMenu = panel.AddComponent<PopUpContextMenu>();
        ctxMenu.Initialize(target, getContentTransform(panel));
        activeMenus.Add(ctxMenu);
        if (blocking)
            BlockUIManager.Instance.Block(ContextMenuCanvas.Instance.Canvas, ctxMenu.Close);
        return ctxMenu;
    }

    private void ApplyGlobalSettings(PopUpContextMenu menu)
    {
        menu.EdgePadding = EdgePadding;
        menu.SideMargin = SideMargin;
        menu.TargetClearance = TargetClearance;
        menu.MaxHeightRatio = MaxHeightRatio;
        menu.FollowLerp = FollowLerp;
    }

    private void Update()
    {
        for (int i = 0; i < activeMenus.Count; i++)
        {
            UpdateContextMenu(activeMenus[i]);
        }
    }

    private void UpdateContextMenu(PopUpContextMenu menu, bool instant = false)
    {
        if (menu == null || menu.TargetTransform == null) return;

        var cam = Camera.main;
        if (cam == null) return;

        var scrollRect = menu.GetComponent<ScrollRect>();
        if (scrollRect == null) return;

        var scrollRectRT = scrollRect.transform as RectTransform;

        // 1) Content: height = sum of children (preferred height of VerticalLayoutGroup)
        LayoutRebuilder.ForceRebuildLayoutImmediate(menu.ContentTransform);
        float contentPreferredHeight = LayoutUtility.GetPreferredHeight(menu.ContentTransform);

        var contentLE = menu.ContentTransform.GetComponent<LayoutElement>();
        if (contentLE == null) contentLE = menu.ContentTransform.gameObject.AddComponent<LayoutElement>();
        contentLE.flexibleHeight = 0f;
        contentLE.preferredHeight = contentPreferredHeight;

        // 2) View (root) is clamped to screen, not the content
        float maxScreenHeight = Screen.height * menu.MaxHeightRatio;
        float scaleFactor = 1f; // assuming no additional scaling with the hosting ScrollRect
        float maxCanvasHeight = maxScreenHeight / scaleFactor;

        var viewLE = menu.RectTransform.GetComponent<LayoutElement>();
        if (viewLE == null) viewLE = menu.RectTransform.gameObject.AddComponent<LayoutElement>();
        viewLE.flexibleHeight = 0f;
        viewLE.preferredHeight = Mathf.Min(contentPreferredHeight, maxCanvasHeight);

        // Rebuild after applying preferred sizes
        LayoutRebuilder.ForceRebuildLayoutImmediate(menu.RectTransform);

        // Menu size for placement/clamping
        Vector2 menuSizePixels = menu.RectTransform.rect.size * scaleFactor;

        // 3) Pick side and position
        Rect targetScreenRect = GetTargetScreenRect(menu.TargetTransform, cam, menu.TargetClearance);

        float leftSpace = Mathf.Max(0f, targetScreenRect.xMin);
        float rightSpace = Mathf.Max(0f, Screen.width - targetScreenRect.xMax);
        float topSpace = Mathf.Max(0f, Screen.height - targetScreenRect.yMax);
        float bottomSpace = Mathf.Max(0f, targetScreenRect.yMin);

        bool canRight = rightSpace >= menuSizePixels.x + menu.SideMargin;
        bool canLeft = leftSpace >= menuSizePixels.x + menu.SideMargin;
        bool canTop = topSpace >= menuSizePixels.y + menu.SideMargin;
        bool canBottom = bottomSpace >= menuSizePixels.y + menu.SideMargin;

        Side chosen;
        if (canRight || canLeft || canTop || canBottom)
        {
            if (canRight) chosen = Side.Right;
            else if (canLeft) chosen = Side.Left;
            else if (canTop) chosen = Side.Top;
            else chosen = Side.Bottom;
        }
        else
        {
            float maxH = Mathf.Max(leftSpace, rightSpace);
            float maxV = Mathf.Max(topSpace, bottomSpace);
            chosen = (maxH >= maxV) ? (rightSpace >= leftSpace ? Side.Right : Side.Left)
                                    : (topSpace >= bottomSpace ? Side.Top : Side.Bottom);
        }

        Vector2 desiredScreenPos;
        Vector2 pivot;

        float midY = Mathf.Clamp(
            (targetScreenRect.yMin + targetScreenRect.yMax) * 0.5f,
            menu.EdgePadding + menuSizePixels.y * 0.5f,
            Screen.height - (menu.EdgePadding + menuSizePixels.y * 0.5f)
        );

        float midX = Mathf.Clamp(
            (targetScreenRect.xMin + targetScreenRect.xMax) * 0.5f,
            menu.EdgePadding + menuSizePixels.x * 0.5f,
            Screen.width - (menu.EdgePadding + menuSizePixels.x * 0.5f)
        );

        switch (chosen)
        {
            case Side.Right:
                pivot = new Vector2(0f, 0.5f);
                desiredScreenPos = new Vector2(
                    Mathf.Min(targetScreenRect.xMax + menu.SideMargin, Screen.width - (menu.EdgePadding + menuSizePixels.x)),
                    midY);
                break;
            case Side.Left:
                pivot = new Vector2(1f, 0.5f);
                desiredScreenPos = new Vector2(
                    Mathf.Max(targetScreenRect.xMin - menu.SideMargin, menu.EdgePadding + menuSizePixels.x),
                    midY);
                break;
            case Side.Top:
                pivot = new Vector2(0.5f, 0f);
                desiredScreenPos = new Vector2(
                    midX,
                    Mathf.Min(targetScreenRect.yMax + menu.SideMargin, Screen.height - (menu.EdgePadding + menuSizePixels.y)));
                break;
            default: // Bottom
                pivot = new Vector2(0.5f, 1f);
                desiredScreenPos = new Vector2(
                    midX,
                    Mathf.Max(targetScreenRect.yMin - menu.SideMargin, menu.EdgePadding + menuSizePixels.y));
                break;
        }

        // Center-space in the hosting ScrollRect
        menu.RectTransform.anchorMin = menu.RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        menu.RectTransform.pivot = pivot;

        float clampedX = Mathf.Clamp(desiredScreenPos.x,
            menu.EdgePadding + menuSizePixels.x * pivot.x,
            Screen.width - (menu.EdgePadding + menuSizePixels.x * (1f - pivot.x)));
        float clampedY = Mathf.Clamp(desiredScreenPos.y,
            menu.EdgePadding + menuSizePixels.y * pivot.y,
            Screen.height - (menu.EdgePadding + menuSizePixels.y * (1f - pivot.y)));
        Vector2 clampedScreen = new Vector2(clampedX, clampedY);

        // Convert to ScrollRect local and move
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRectRT, clampedScreen, null, out localPoint);

        if (instant)
        {
            menu.RectTransform.anchoredPosition = localPoint;
            if (menu.CanvasGroup != null)
            {
                menu.CanvasGroup.alpha = 1f;
            }
        }
        else
        {
            menu.RectTransform.anchoredPosition = Vector2.Lerp(
                menu.RectTransform.anchoredPosition,
                localPoint,
                1f - Mathf.Pow(1f - menu.FollowLerp, Time.unscaledDeltaTime * 60f));
        }
    }

    // Build a best-effort screen rect for the target
    private static Rect GetTargetScreenRect(Transform target, Camera cam, float minSizePixels)
    {
        var col2d = target.GetComponent<Collider2D>();
        if (col2d != null) return WorldBoundsToScreenRect(col2d.bounds, cam, minSizePixels);

        var col3d = target.GetComponent<Collider>();
        if (col3d != null) return WorldBoundsToScreenRect(col3d.bounds, cam, minSizePixels);

        var rend = target.GetComponent<Renderer>();
        if (rend != null) return WorldBoundsToScreenRect(rend.bounds, cam, minSizePixels);

        var rt = target as RectTransform;
        if (rt != null)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Bounds b = new Bounds(corners[0], Vector3.zero);
            for (int i = 1; i < 4; i++) b.Encapsulate(corners[i]);
            return WorldBoundsToScreenRect(b, cam, minSizePixels);
        }

        Vector3 sp = cam.WorldToScreenPoint(target.position);
        float half = Mathf.Max(16f, minSizePixels * 0.5f);
        return new Rect(sp.x - half, sp.y - half, half * 2f, half * 2f);
    }

    private static Rect WorldBoundsToScreenRect(Bounds worldBounds, Camera cam, float minSizePixels)
    {
        Vector3 c = worldBounds.center;
        Vector3 e = worldBounds.extents;

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        for (int xi = -1; xi <= 1; xi += 2)
        for (int yi = -1; yi <= 1; yi += 2)
        for (int zi = -1; zi <= 1; zi += 2)
        {
            Vector3 corner = c + Vector3.Scale(e, new Vector3(xi, yi, zi));
            Vector3 sp = cam.WorldToScreenPoint(corner);
            min = Vector2.Min(min, new Vector2(sp.x, sp.y));
            max = Vector2.Max(max, new Vector2(sp.x, sp.y));
        }

        Rect r = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

        if (r.width < minSizePixels)
        {
            float pad = (minSizePixels - r.width) * 0.5f;
            r.xMin -= pad; r.xMax += pad;
        }
        if (r.height < minSizePixels)
        {
            float pad = (minSizePixels - r.height) * 0.5f;
            r.yMin -= pad; r.yMax += pad;
        }

        return r;
    }

    public static Vector2 worldScreenHalfSize(Camera cam)
    {
        if (cam.orthographic)
        {
            float height = cam.orthographicSize;
            float width = height * cam.aspect;

            return new Vector2(width, height);
        }
        else
        {
            float height = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * -cam.transform.position.z;
            float width = height * cam.aspect;
            return new Vector2(width, height);
        }
    }

    public static Rect cameraWorldSizeRect(Camera cam)
    {
        var halfSize = worldScreenHalfSize(cam);
        Rect rect = new Rect(Vector2.zero, halfSize * 2f);
        rect.center = cam.transform.position;
        return rect;
    }

    private void TestObjectSelected(ObjectWithPopupMenuTest objectWithProvider)
    {
        ObjectWithPopupMenuTest clickable = objectWithProvider as ObjectWithPopupMenuTest;
        if (clickable != null)
        {
            RectTransform panel = Instantiate(popUpMenuPrefab).GetComponent<RectTransform>();
            panel.position = objectWithProvider.transform.position;
        }
    }
}