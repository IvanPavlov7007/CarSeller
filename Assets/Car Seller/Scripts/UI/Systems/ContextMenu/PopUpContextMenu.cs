using UnityEngine;

public class PopUpContextMenu : MonoBehaviour, IClosable
{
    public RectTransform RectTransform { get; private set; }
    public Transform TargetTransform { get; private set; }
    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Transform targetTransform)
    {
        TargetTransform = targetTransform;
    }

    public void Close()
    {
        ContextMenuManager.Instance.closeMenu(this);
    }
}