using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.Utilities;

[CreateAssetMenu(fileName = "UIBuilder", menuName = "GameUI/UIBuilder")]
public class SimpleUIBuilder : SingletonScriptableObject<SimpleUIBuilder>, IUIElementBuilder<RectTransform>
{
    public GameObject RowHolderPrefab;
    public GameObject PushButtonPrefab;
    public GameObject TextMeshProPrefab;
    public GameObject ImagePrefab;
    public GameObject VLayoutPrefab;
    public GameObject HLayoutPrefab;
    public GameObject GridLayoutPrefab;
    public RectTransform Build(UIElement content, RectTransform container)
    {
        switch(content.Type)
        {
            case UIElementType.Container:
                foreach(var child in content.Children)
                {
                    Build(child, container);
                }
                return container;
            case UIElementType.Button:
                return BuildButton(content, container);
            case UIElementType.Text:
                return BuildText(content, container);
            default:
                return null;
        }
    }

    private RectTransform BuildText(UIElement item, RectTransform container)
    {
        RectTransform rowHolder = GameObject.Instantiate(RowHolderPrefab, container).GetComponent<RectTransform>();
        var headerTMPObj = GameObject.Instantiate(TextMeshProPrefab, rowHolder);
        var headerTMP = headerTMPObj.GetComponent<TextMeshProUGUI>();
        headerTMP.text = item.Text;
        return rowHolder;
    }

    private RectTransform BuildButton(UIElement item, RectTransform container)
    {
        RectTransform rowHolder = GameObject.Instantiate(RowHolderPrefab, container).GetComponent<RectTransform>();
        var buttonObj = GameObject.Instantiate(PushButtonPrefab, rowHolder);
        var buttonTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        buttonTMP.text = item.Text;
        buttonObj.AddComponent<ButtonStateController>().
             Initialize(item);
        return rowHolder;
    }
}