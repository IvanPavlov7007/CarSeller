using UnityEngine;
using UnityEngine.UI;

public class PopUpContextMenu : MonoBehaviour, IClosable
{
    public RectTransform RectTransform { get; private set; }
    public RectTransform ContentTransform { get; private set; }
    public Transform TargetTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Transform targetTransform, RectTransform contentTransform)
    {
        TargetTransform = targetTransform;
        ContentTransform = contentTransform;
    }

    public void Close()
    {
        ContextMenuManager.Instance.closeMenu(this);
    }
}