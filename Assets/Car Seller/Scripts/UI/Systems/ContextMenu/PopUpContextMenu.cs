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

    public void Initialize(Transform targetTransform)
    {
        TargetTransform = targetTransform;
    }

    public void Initialize(Transform targetTransform, RectTransform contentTransform)
    {
        TargetTransform = targetTransform;
        ContentTransform = contentTransform;

        // Ensure the panel starts fully visible (animation is handled by manager if needed)
        if (CanvasGroup != null) CanvasGroup.alpha = 1f;

        // Optional: make sure panel sizes to its content
        var fitter = GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    public void Close()
    {
        ContextMenuManager.Instance.closeMenu(this);
    }
}