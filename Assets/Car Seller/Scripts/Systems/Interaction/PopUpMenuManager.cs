using System.Collections;
using UnityEngine;
using Pixelplacement;

public class PopUpMenuManager : Singleton<PopUpMenuManager>
{
    public Vector2 offsetFromPosition = new Vector2(0, 10);
    public GameObject popUpMenuPrefab;

    IRectFillBuilder contextMenuBuilder => SimpleUIBuilder.Instance;


    private void OnEnable()
    {
        GameEvents.Instance.OnObjectWithPopupClicked += TestObjectSelected;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnObjectWithPopupClicked -= TestObjectSelected;
    }

    public void CreateContextMenu(GameObject target, UIContentList content)
    {
        RectTransform panel = Instantiate(popUpMenuPrefab).GetComponent<RectTransform>();
        panel.position = target.transform.position + (Vector3)offsetFromPosition;
        SimpleUIBuilder.Instance.Build(panel.GetChild(0).GetChild(0) as RectTransform, content);
        BlockUIManager.Instance.Block(panel.GetComponent<Canvas>,);
    }


    // Temporary test methods
    private void TestObjectSelected(ObjectWithPopupMenuTest objectWithProvider)
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
        SimpleUIBuilder.Instance.Build(container, contents);
    }



    


}