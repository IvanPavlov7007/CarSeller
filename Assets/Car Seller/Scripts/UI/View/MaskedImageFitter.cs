using UnityEngine;
using UnityEngine.UI;

//ChatGPT generated code
[RequireComponent(typeof(RectMask2D))]
public class MaskedImageFitter : MonoBehaviour
{
    [SerializeField] private RectTransform imageRect;
    [SerializeField] private Image image;

    private RectTransform _maskRect;
    private bool _initialized;

    private void Awake()
    {
        _maskRect = GetComponent<RectTransform>();
        if (imageRect == null)
        {
            imageRect = GetComponentInChildren<Image>()?.rectTransform;
        }
        if (image == null && imageRect != null)
        {
            image = imageRect.GetComponent<Image>();
        }
    }

    private void LateUpdate()
    {
        if (imageRect == null || image == null || image.sprite == null)
            return;

        var sprite = image.sprite;
        if (sprite.rect.height <= 0f)
            return;

        // width of the mask in local space
        float maskWidth = _maskRect.rect.width;
        if (maskWidth <= 0f)
            return;

        // sprite aspect ratio (w/h)
        float spriteAspect = sprite.rect.width / sprite.rect.height;

        // desired height so that width matches mask and aspect is preserved
        float targetHeight = maskWidth / spriteAspect;

        // apply
        var size = imageRect.sizeDelta;
        size.x = maskWidth;
        size.y = targetHeight;
        imageRect.sizeDelta = size;

        // center the image so mask crops top/bottom
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;

        // ensure aspect is preserved
        image.preserveAspect = true;
    }
}