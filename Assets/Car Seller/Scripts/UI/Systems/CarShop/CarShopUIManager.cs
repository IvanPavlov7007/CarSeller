using Pixelplacement;
using System;
using UnityEngine;

public class CarShopUIManager : Singleton<CarShopUIManager>
{
    public RectTransform content;
    public SimpleUIBuilder uiBuilder;

    public void UpdateOffers(CarShopOffer[] offers)
    {
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }
        var rootElement = GenerateUIElements(offers);
        uiBuilder.Build(rootElement, content);
    }

    private UIElement GenerateUIElements(CarShopOffer[] offers)
    {
        UIElement rootElement = new UIElement
        {
            Type = UIElementType.Container,
            Children = new System.Collections.Generic.List<UIElement>()
        };

        foreach(var offer in offers)
        {
            UIElement offerElement = new UIElement
            {
                Type = UIElementType.Button,
                Text = $"Exchange for {offer.ReceivedCar.Name} (Money Delta: {offer.MoneyDelta})",
                IsInteractable = offer.CanAccept(),
                OnClick = () =>
                {
                    if (offer.CanAccept())
                    {
                        offer.Accept();
                        Debug.Log($"Offer accepted: Exchanged for {offer.ReceivedCar.Name}");
                    }
                    else
                    {
                        Debug.Log("Offer cannot be accepted.");
                    }
                }
            };
            rootElement.Children.Add(offerElement);
        }
        return rootElement;
    }
}