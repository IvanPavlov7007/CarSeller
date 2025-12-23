using Pixelplacement;
using UnityEngine;
public class ContextMenuCanvas : Singleton<ContextMenuCanvas>
{
    public RectTransform RectTransform { get; private set; }
    public Canvas Canvas { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }

    private void Awake()
    {
        Canvas = GetComponent<Canvas>();
        CanvasGroup = GetComponent<CanvasGroup>();
        RectTransform = GetComponent<RectTransform>();
    }
}