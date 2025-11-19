using System.Collections;
using UnityEngine;
using Pixelplacement;

public class PopUpMenuManager : Singleton<PopUpMenuManager>
{
    public void ShowPopUpMenu(RectTransform container, UIContentProvider contentProvider, UIContext context)
    {
        var contents = contentProvider.GetContents(context);
        UIBuilder.Instance.Build(container, contents);
    }
}