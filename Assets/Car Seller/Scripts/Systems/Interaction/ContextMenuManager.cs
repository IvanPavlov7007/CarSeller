using System.Collections;
using UnityEngine;
using Pixelplacement;
using System.Collections.Generic;

public class ContextMenuManager : Singleton<ContextMenuManager>
{
    public Vector2 offsetUp = new Vector2(0, 10);
    public Vector2 offsetDown = new Vector2(0, -10);
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

    private void OnEnable()
    {
        GameEvents.Instance.OnObjectWithPopupClicked += TestObjectSelected;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnObjectWithPopupClicked -= TestObjectSelected;
    }

    public void CreateContextMenu(GameObject target, UIElement content)
    {
        GameObject panel = Instantiate(popUpMenuPrefab);
        RectTransform panelTransform = panel.GetComponent<RectTransform>();

        var offset = target.transform.position.y < 0 ? offsetUp : offsetDown;
        panelTransform.position = target.transform.position + (Vector3)offset;
        SimpleUIBuilder.Instance.Build(content,panelTransform.GetChild(0).GetChild(0) as RectTransform);
        createContextMenu(panel, content.blockingInput);
    }

    private PopUpContextMenu createContextMenu(GameObject panel, bool blocking)
    {
        var ctxMenu = panel.AddComponent<PopUpContextMenu>();
        activeMenus.Add(ctxMenu);
        if(blocking)
            BlockUIManager.Instance.Block(panel.GetComponent<Canvas>(), ctxMenu.Close);
        return ctxMenu;
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