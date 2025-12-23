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
    public const float MinRowHeight = 64f;
    [Tooltip("Minimum button height to provide a comfortable touch target.")]
    [ShowInInspector]
    public const float MinButtonHeight = 120f;
    [ShowInInspector]
    public const float MinImageHeight = 400f;

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
                headerTMP.fontSize = 48;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Top;
                break;
            case "description":
                headerTMP.fontSize = 32;
                headerTMP.fontStyle = FontStyles.Italic;
                headerTMP.alignment = TextAlignmentOptions.Left;
                break;
            case "hint":
                headerTMP.fontSize = 32;
                headerTMP.color = Color.gray;
                headerTMP.alignment = TextAlignmentOptions.Right;
                break;
            case "price":
                headerTMP.fontSize = 38;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Baseline;
                headerTMP.color = Color.white;
                break;
            default:
                headerTMP.fontSize = 24; // bump default for readability on touch
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
        EnsureMinHeight(recT, MinButtonHeight);
        return recT;
    }

    private RectTransform BuildImage(UIElement item, RectTransform container)
    {
        var imageObj = GameObject.Instantiate(ImagePrefab, container);
        var image = imageObj.GetComponent<Image>();
        image.sprite = item.Image;

        RectTransform recT = imageObj.GetComponent<RectTransform>();
        EnsureMinHeight(recT, MinImageHeight);
        return recT;
    }

    private void EnsureMinHeight(RectTransform rowHolder, float minHeight = MinRowHeight)
    {
        var le = rowHolder.GetComponent<LayoutElement>();
        if (le == null) le = rowHolder.gameObject.AddComponent<LayoutElement>();
        if (le.minHeight < minHeight) le.minHeight = minHeight;
        // Disallow flexible shrinking to preserve touch targets
        le.flexibleHeight = 0f;
    }
}