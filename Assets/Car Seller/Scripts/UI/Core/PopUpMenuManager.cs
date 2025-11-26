using System.Collections;
using UnityEngine;
using Pixelplacement;

public class PopUpMenuManager : Singleton<PopUpMenuManager>
{
    public Vector2 offsetFromPosition = new Vector2(0, 10);
    public GameObject popUpMenuPrefab;

    private void OnEnable()
    {
        GameEvents.Instance.OnObjectWithPopupClicked += ObjectSelected;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnObjectWithPopupClicked -= ObjectSelected;
    }

    public void ObjectSelected(ObjectWithPopupMenuTest objectWithProvider)
    {
        ObjectWithPopupMenuTest clickable = objectWithProvider as ObjectWithPopupMenuTest;
        if(clickable != null)
        {
            RectTransform panel = Instantiate(popUpMenuPrefab).GetComponent<RectTransform>();
            panel.position = objectWithProvider.transform.position + (Vector3) offsetFromPosition;
            ShowPopUpMenu(panel.GetChild(0).GetChild(0) as RectTransform, clickable.contentProvider, new UIContext {} );
        }

    }

    public void ShowPopUpMenu(RectTransform container, UIContentProvider contentProvider, UIContext context)
    {
        var contents = contentProvider.GetContents(context);
        UIBuilder.Instance.Build(container, contents);
    }
}