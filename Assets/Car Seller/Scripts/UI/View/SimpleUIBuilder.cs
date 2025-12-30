using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "UIBuilder", menuName = "GameUI/UIBuilder")]
public class SimpleUIBuilder : SingletonScriptableObject<SimpleUIBuilder>, IUIElementBuilder<RectTransform>
{
    [Header("Prefabs")]
    public GameObject RowHolderPrefab;
    public GameObject PushButtonPrefab;
    public GameObject TextMeshProPrefab;
    public GameObject ImagePrefab;
    public GameObject VLayoutPrefab;
    public GameObject HLayoutPrefab;
    public GameObject GridLayoutPrefab;

    [Header("Touch Sizing")]
    [Tooltip("Minimum row height to provide a comfortable touch target.")]
    [ShowInInspector]
    public const float MinRowHeight = 32f;
    [Tooltip("Minimum button height to provide a comfortable touch target.")]
    [ShowInInspector]
    public const float MinButtonHeight = 60f;
    [ShowInInspector]
    public const float MinImageHeight = 200f;
    [ShowInInspector]
    public const float PrefferedButtonHeight = 80f;

    public RectTransform Build(UIElement content, RectTransform container)
    {
        switch (content.Type)
        {
            case UIElementType.Container:
                foreach (var child in content.Children)
                {
                    Build(child, container);
                }
                return container;
            case UIElementType.Button:
                return BuildButton(content, container);
            case UIElementType.Text:
                return BuildText(content, container);
            case UIElementType.Image:
                return BuildImage(content, container);
            default:
                return null;
        }
    }
    

    private RectTransform BuildText(UIElement item, RectTransform container)
    {
        var headerTMPObj = GameObject.Instantiate(TextMeshProPrefab, container);
        var headerTMP = headerTMPObj.GetComponent<TextMeshProUGUI>();
        headerTMP.text = item.Text;
        switch (item.Style)
        {
            case "header":
                headerTMP.fontSize = 56;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Top;
                break;
            case "description":
                headerTMP.fontSize = 48;
                headerTMP.fontStyle = FontStyles.Italic;
                headerTMP.alignment = TextAlignmentOptions.MidlineLeft;
                break;
            case "hint":
                headerTMP.fontSize = 48;
                headerTMP.color = Color.gray;
                headerTMP.alignment = TextAlignmentOptions.MidlineRight;
                break;
            case "price":
                headerTMP.fontSize = 56;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Midline;
                headerTMP.color = Color.white;
                break;
            default:
                headerTMP.fontSize = 48; // bump default for readability on touch
                break;
        }

        // Make TMP auto-size to use available vertical space
        //headerTMP.enableAutoSizing = true;
        headerTMP.textWrappingMode = TextWrappingModes.Normal;

        RectTransform recT = headerTMP.GetComponent<RectTransform>();

        // Ensure row has at least MinRowHeight, but expand to fit text if needed
        LayoutRebuilder.ForceRebuildLayoutImmediate(recT);
        var preferredHeight = headerTMP.preferredHeight;

        var le = recT.GetComponent<LayoutElement>();
        if (le == null) le = recT.gameObject.AddComponent<LayoutElement>();

        le.minHeight = Mathf.Max(MinRowHeight, preferredHeight);
        le.preferredHeight = Mathf.Max(le.preferredHeight, preferredHeight);
        le.flexibleHeight = 0f;

        return recT;
    }

    private RectTransform BuildButton(UIElement item, RectTransform container)
    {
        var buttonObj = GameObject.Instantiate(PushButtonPrefab, container);
        var buttonTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTMP != null)
        {
            buttonTMP.text = item.Text;
            buttonTMP.fontSize = Mathf.Max(buttonTMP.fontSize, 20); // comfortable default
        }

        buttonObj.AddComponent<ButtonStateController>().
             Initialize(item);

        RectTransform recT = buttonObj.GetComponent<RectTransform>();
        // Ensure button itself has adequate hit size via LayoutElement
        EnsureMinHeight(recT, MinButtonHeight, PrefferedButtonHeight);
        return recT;
    }

    private RectTransform BuildImage(UIElement item, RectTransform container)
    {
        // Parent object that will provide the mask and layout size
        var maskObj = new GameObject("ImageMask", typeof(RectTransform), typeof(RectMask2D), typeof(MaskedImageFitter));
        var maskRect = maskObj.GetComponent<RectTransform>();
        maskRect.SetParent(container, false);

        // Stretch horizontally in the row
        maskRect.anchorMin = new Vector2(0f, 0.5f);
        maskRect.anchorMax = new Vector2(1f, 0.5f);
        maskRect.pivot = new Vector2(0.5f, 0.5f);
        maskRect.anchoredPosition = Vector2.zero;

        // Ensure minimum height for touch
        EnsureMinHeight(maskRect, MinImageHeight);

        // Horizontal band: width decides height (e.g. 4:3)
        var aspect = maskObj.AddComponent<AspectRatioFitter>();
        aspect.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        aspect.aspectRatio = 4f / 3f; // desired mask aspect

        // Actual image inside the mask – start centered
        var imageObj = GameObject.Instantiate(ImagePrefab, maskRect);
        var imageRect = imageObj.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;

        var image = imageObj.GetComponent<Image>();
        image.sprite = item.Image;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;

        // Configure fitter so it knows which image to size
        var fitter = maskObj.GetComponent<MaskedImageFitter>();
        var fitterImageField = typeof(MaskedImageFitter).GetField("image", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fitterImageRectField = typeof(MaskedImageFitter).GetField("imageRect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fitterImageField != null) fitterImageField.SetValue(fitter, image);
        if (fitterImageRectField != null) fitterImageRectField.SetValue(fitter, imageRect);

        return maskRect;
    }

    private void EnsureMinHeight(RectTransform rowHolder, float minHeight = MinRowHeight, float prefferedHeight = -1f)
    {
        var le = rowHolder.GetComponent<LayoutElement>();
        if (le == null) le = rowHolder.gameObject.AddComponent<LayoutElement>();
        if (le.minHeight < minHeight) le.minHeight = minHeight;
        // Disallow flexible shrinking to preserve touch targets
        le.flexibleHeight = 0f;
        if(prefferedHeight > 0f)
            le.preferredHeight = prefferedHeight;
    }
}