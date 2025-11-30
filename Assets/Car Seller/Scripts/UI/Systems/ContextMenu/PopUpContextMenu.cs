using UnityEngine;

public class PopUpContextMenu : MonoBehaviour, IClosable
{
    public void Close()
    {
        ContextMenuManager.Instance.closeMenu(this);
    }
}