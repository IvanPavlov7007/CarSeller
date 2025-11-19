using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.Utilities;

[CreateAssetMenu(fileName = "UIBuilder", menuName = "GameUI/UIBuilder")]
public class UIBuilder : SingletonScriptableObject<UIBuilder>
{
    public GameObject RowHolderPrefab;
    public GameObject PushButtonPrefab;
    public GameObject TextMeshProPrefab;
    public GameObject ImagePrefab;
    public GameObject VLayoutPrefab;
    public GameObject HLayoutPrefab;
    public GameObject GridLayoutPrefab;


    public void Build(RectTransform container, IEnumerable<UIContent> contents)
    {
        //delete all children
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in contents)
        {
            Transform rowHolder = GameObject.Instantiate(RowHolderPrefab, container).transform;
            switch (item.ContentType)
            {
                case UIContentType.Header:
                    break;
                case UIContentType.Text:
                    break;
                case UIContentType.Button:
                    var buttonObj = GameObject.Instantiate(PushButtonPrefab, rowHolder);
                    var buttonTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                    var button = buttonObj.GetComponent<Button>();
                    buttonTMP.text = string.IsNullOrEmpty(item.Header) ? item.Text : item.Header;
                    button.onClick.AddListener(() => item.pushAction?.Invoke());
                    break;
                default:
                    break;
            }
        }
    }
}