using System.Collections;
using UnityEngine;
using Pixelplacement;
using System.Collections.Generic;

public class ContextMenuManager : Singleton<ContextMenuManager>
{
    public GameObject popUpMenuPrefab;

    List<PopUpContextMenu> activeMenus = new List<PopUpContextMenu>();

    public void closeMenu(PopUpContextMenu menu)
    {
        if(activeMenus.Contains(menu))
        {
            activeMenus.Remove(menu);
            BlockUIManager.Instance.Unblock(menu.GetComponent<Canvas>());
            Destroy(menu.gameObject);
        }
    }

    public void CreateContextMenu(GameObject target, UIElement content)
    {
        GameObject panel = Instantiate(popUpMenuPrefab, ContextMenuCanvas.Instance.RectTransform);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();

        SimpleUIBuilder.Instance.Build(content, getContentTransform(panel));
        createContextMenu(panel, target.transform, content.blockingInput);
        UpdateContextMenu(panel.GetComponent<PopUpContextMenu>());
    }

    private RectTransform getContentTransform(GameObject panel)
    {
        return panel.transform.GetChild(0).GetChild(0) as RectTransform;
    }   

    private PopUpContextMenu createContextMenu(GameObject panel, Transform target, bool blocking)
    {
        var ctxMenu = panel.AddComponent<PopUpContextMenu>();
        ctxMenu.Initialize(target);
        activeMenus.Add(ctxMenu);
        if(blocking)
            BlockUIManager.Instance.Block(panel.GetComponent<Canvas>(), ctxMenu.Close);
        return ctxMenu;
    }

    private void UpdateContextMenu(PopUpContextMenu menu)
    {

    }


    private void Update()
    {
        foreach(var menu in activeMenus)
        {
            UpdateContextMenu(menu);
        }
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


    // Temporary test methods
    private void TestObjectSelected(ObjectWithPopupMenuTest objectWithProvider)
    {
        ObjectWithPopupMenuTest clickable = objectWithProvider as ObjectWithPopupMenuTest;
        if(clickable != null)
        {
            RectTransform panel = Instantiate(popUpMenuPrefab).GetComponent<RectTransform>();
            panel.position = objectWithProvider.transform.position + (Vector3) offsetUp;
        }

    }
}