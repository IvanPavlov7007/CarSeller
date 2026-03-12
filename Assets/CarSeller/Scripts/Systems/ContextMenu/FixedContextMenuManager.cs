using Pixelplacement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Might be used in the future for fixed context menus
// TODO don't duplicate code with ContextMenuManager
public class FixedContextMenuManager : Singleton<FixedContextMenuManager>
{
    public GameObject fixedPopUpMenuPrefab;
    public SimpleUIBuilder UIBuilder;
    public HashSet<PopUpContextMenu> activeMenus = new HashSet<PopUpContextMenu>();

    [Header("Context Menu Visuals")]
    [SerializeField]
    private Color UsedContextMenuColor = Color.white;

    public PopUpContextMenu CreateContextMenu(UIElement content)
    {
        GameObject panel = Instantiate(fixedPopUpMenuPrefab, FixedContextMenuCanvas.Instance.Canvas.transform);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();

        // Set panel color
        var image = panelTransform.GetComponent<Image>();
        if (image != null)
        {
            image.color = UsedContextMenuColor;
        }

        var contentTransform = getContentTransform(panel);
        UIBuilder.Build(content, contentTransform);

        var menu = createContextMenu(panel, content.blockingInput);
        menu.Initialize(null, contentTransform, OnMenuClose);

        GameCursor.Instance.CancelCurrentInteraction(invokeDragEnd: false);
        return menu;
    }

    public PopUpContextMenu CreateContextMenu(Widget widget)
    {
        RectTransform view = UIBuilder.Build(widget, FixedContextMenuCanvas.Instance.Canvas.transform);

        var menu = createContextMenu(view.gameObject, widget.BlockingInput);
        menu.Initialize(null, view, OnMenuClose);

        GameCursor.Instance.CancelCurrentInteraction(invokeDragEnd: false);
        return menu;
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