using Pixelplacement;
using System;
using System.Collections.Generic;
using Unity;
using UnityEngine;
using UnityEngine.UI;

// Might be used in the future for fixed context menus
// TODO don't duplicate code with ContextMenuManager
public class FixedContextMenuManager : Singleton<FixedContextMenuManager>
{
    public GameObject fixedPopUpMenuPrefab;
    public SimpleUIBuilder UIBuilder;
    public HashSet<PopUpContextMenu> activeMenus = new HashSet<PopUpContextMenu>();


    public void CreateContextMenu(UIElement content)
    {
        GameObject panel = Instantiate(fixedPopUpMenuPrefab, FixedContextMenuCanvas.Instance.Canvas.transform);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();
        var contentTransform = getContentTransform(panel);
        UIBuilder.Build(content, contentTransform);
        var menu = createContextMenu(panel, content.blockingInput);
        menu.Initialize(null, contentTransform, OnMenuClose);
        GameCursor.Instance.CancelCurrentInteraction();
    }

    private PopUpContextMenu createContextMenu(GameObject panel, bool blocking)
    {
        var ctxMenu = panel.AddComponent<PopUpContextMenu>();
        activeMenus.Add(ctxMenu);
        if (blocking)
            BlockUIManager.Instance.Block(FixedContextMenuCanvas.Instance.Canvas, ctxMenu.Close);
        return ctxMenu;
    }

    private void OnMenuClose(PopUpContextMenu menu)
    {
        if (activeMenus.Contains(menu))
        {
            activeMenus.Remove(menu);
            BlockUIManager.Instance.Unblock(FixedContextMenuCanvas.Instance.Canvas);
            Destroy(menu.gameObject);
        }
    }

    private RectTransform getContentTransform(GameObject panel)
    {
        // Using a LayoutGroup child as Content (VerticalLayoutGroup present)
        return panel.GetComponentInChildren<LayoutGroup>().GetComponent<RectTransform>();
    }
}