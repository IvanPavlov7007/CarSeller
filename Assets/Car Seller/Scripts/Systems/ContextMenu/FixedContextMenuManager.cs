using Pixelplacement;
using Unity;
using UnityEngine;

// Might be used in the future for fixed context menus

public class FixedContextMenuManager : Singleton<FixedContextMenuManager>
{
    public GameObject fixedPopUpMenuPrefab;
    public void CreateContextMenu(GameObject target, UIElement content)
    {
        GameObject panel = Instantiate(fixedPopUpMenuPrefab, ContextMenuCanvas.Instance.Canvas.transform);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();
    }
}