using System.Collections;
using UnityEngine;
using Pixelplacement;
using System.Collections.Generic;
using UnityEngine.UI; // for LayoutRebuilder

public class ContextMenuManager : Singleton<ContextMenuManager>
{
    public GameObject popUpMenuPrefab;
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

        UpdateContextMenu(ctxMenu);
    }

    private RectTransform getContentTransform(GameObject panel)
    {
        // Using a LayoutGroup child as Content (VerticalLayoutGroup present)
        return panel.GetComponentInChildren<LayoutGroup>().GetComponent<RectTransform>();
    }

    private void adjustViewRect(RectTransform view, RectTransform content)
    {
        if (view.sizeDelta.y > content.sizeDelta.y)
        {
            var size = view.sizeDelta;
            size.y = content.sizeDelta.y;
            view.sizeDelta = size;
        }
    }

    private void adjustContainerHeight(RectTransform container)
    {
        float sum = 0f;
        foreach (RectTransform child in container)
        {
            sum += child.rect.height;
        }
        // Instead of assigning to container.rect (which is read-only), set sizeDelta
        var sizeDelta = container.sizeDelta;
        sizeDelta.y = sum;
        container.sizeDelta = sizeDelta;
    }

    private PopUpContextMenu createContextMenu(GameObject panel, Transform target, bool blocking)
    {
        var ctxMenu = panel.AddComponent<PopUpContextMenu>();
        ctxMenu.Initialize(target, getContentTransform(panel));
        activeMenus.Add(ctxMenu);
        if (blocking)
            BlockUIManager.Instance.Block(ContextMenuCanvas.Instance.Canvas, ctxMenu.Close);
        return ctxMenu;
    }

    private void LateUpdate()
    {
        for (int i = 0; i < activeMenus.Count; i++)
        {
            UpdateContextMenu(activeMenus[i]);
        }
    }

    private void UpdateContextMenu(PopUpContextMenu menu)
    {
        var canvas = ContextMenuCanvas.Instance.Canvas;
        var camera = Camera.main;
        adjustContainerHeight(menu.ContentTransform);
        adjustViewRect(menu.RectTransform, menu.ContentTransform);
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
            // For perspective, use field of view and distance
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
}