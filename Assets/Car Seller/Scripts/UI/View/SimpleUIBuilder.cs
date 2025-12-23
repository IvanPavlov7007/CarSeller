using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.Utilities;

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
    public float MinRowHeight = 64f;
    [Tooltip("Minimum button height to provide a comfortable touch target.")]
    public float MinButtonHeight = 64f;

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
        RectTransform rowHolder = GameObject.Instantiate(RowHolderPrefab, container).GetComponent<RectTransform>();
        EnsureRowMinHeight(rowHolder);

        var headerTMPObj = GameObject.Instantiate(TextMeshProPrefab, rowHolder);
        var headerTMP = headerTMPObj.GetComponent<TextMeshProUGUI>();
        headerTMP.text = item.Text;
        switch (item.Style)
        {
            case "header":
                headerTMP.fontSize = 24;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Top;
                break;
            case "description":
                headerTMP.fontSize = 18;
                headerTMP.fontStyle = FontStyles.Italic;
                headerTMP.alignment = TextAlignmentOptions.Left;
                break;
            case "hint":
                headerTMP.fontSize = 14;
                headerTMP.color = Color.gray;
                headerTMP.alignment = TextAlignmentOptions.Right;
                break;
            case "price":
                headerTMP.fontSize = 20;
                headerTMP.fontStyle = FontStyles.Bold;
                headerTMP.alignment = TextAlignmentOptions.Baseline;
                headerTMP.color = Color.white;
                break;
            default:
                headerTMP.fontSize = 18; // bump default for readability on touch
                break;
        }

        return rowHolder;
    }

    private RectTransform BuildButton(UIElement item, RectTransform container)
    {
        RectTransform rowHolder = GameObject.Instantiate(RowHolderPrefab, container).GetComponent<RectTransform>();
        EnsureRowMinHeight(rowHolder);

        var buttonObj = GameObject.Instantiate(PushButtonPrefab, rowHolder);
        var buttonTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTMP != null)
        {
            buttonTMP.text = item.Text;
            buttonTMP.fontSize = Mathf.Max(buttonTMP.fontSize, 20); // comfortable default
        }

        // Ensure button itself has adequate hit size via LayoutElement
        var le = buttonObj.GetComponent<LayoutElement>();
        if (le == null) le = buttonObj.AddComponent<LayoutElement>();
        le.minHeight = Mathf.Max(le.minHeight, MinButtonHeight);

        buttonObj.AddComponent<ButtonStateController>().
             Initialize(item);
        return rowHolder;
    }

    private RectTransform BuildImage(UIElement item, RectTransform container)
    {
        RectTransform rowHolder = GameObject.Instantiate(RowHolderPrefab, container).GetComponent<RectTransform>();
        EnsureRowMinHeight(rowHolder);

        var imageObj = GameObject.Instantiate(ImagePrefab, rowHolder);
        var image = imageObj.GetComponent<Image>();
        image.sprite = item.Image;
        return rowHolder;
    }

    private void EnsureRowMinHeight(RectTransform rowHolder)
    {
        var le = rowHolder.GetComponent<LayoutElement>();
        if (le == null) le = rowHolder.gameObject.AddComponent<LayoutElement>();
        if (le.minHeight < MinRowHeight) le.minHeight = MinRowHeight;
        // Disallow flexible shrinking to preserve touch targets
        le.flexibleHeight = 0f;
    }
}