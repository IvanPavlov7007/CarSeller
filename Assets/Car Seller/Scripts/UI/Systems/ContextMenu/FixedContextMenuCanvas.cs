using Pixelplacement;
using UnityEngine;

public class FixedContextMenuCanvas : Singleton<FixedContextMenuCanvas>
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