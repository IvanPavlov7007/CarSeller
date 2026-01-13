using System;
using UnityEngine;
using UnityEngine.UI;

public class PopUpContextMenu : MonoBehaviour, IClosable
{
    public RectTransform RectTransform { get; private set; }
    public RectTransform ContentTransform { get; private set; }
    public Transform TargetTransform { get; private set; }

    private event Action<PopUpContextMenu> OnClose;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Transform targetTransform, RectTransform contentTransform, Action<PopUpContextMenu> onClose)
    {
        TargetTransform = targetTransform;
        ContentTransform = contentTransform;
        OnClose += onClose;
    }

    public void Close()
    {
        OnClose?.Invoke(this);
    }
}