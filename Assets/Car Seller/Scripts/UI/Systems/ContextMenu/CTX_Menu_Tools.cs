using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CTX_Menu_Tools
{
    // General
    public static UIElement Header(string name)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"{name}",
            Style = "header"
        };
    }
    public static UIElement Description(string description)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"{description}",
            Style = "description"
        };
    }

    public static UIElement Hint(string text)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"{text}",
            Style = "hint"
        };
    }

    public static UIElement Image(Sprite iconSprite)
    {
        return new UIElement
        {
            Type = UIElementType.Image,
            Image = iconSprite
        };
    }

    public static UIElement CancelButton()
    {
        return new UIElement
        {
            Type = UIElementType.Button,
            Text = "Cancel",
            OnClick = () =>
            {
                GameEvents.Instance.OnPlayerCancel(new PlayerActionEventData(PlayerOutcome.Canceled, null));
            },
        };
    }


    // Warehouse
    public static List<UIElement> WarehouseBaseInfoElements(Warehouse warehouse)
    {
        return new List<UIElement>()
        {
            Header(warehouse.Name),
            Image(warehouse.Config.image),
            WarehouseStats(warehouse),
        };
    }

    public static List<UIElement> WarehousePurchaseElements(WarehouseOffer offer)
    {
        return new List<UIElement>()
        {
            new UIElement
            {
                Type = UIElementType.Text,
                Text = $"{offer.Price}",
                Style = "price"
            },
            new UIElement
            {
                Type = UIElementType.Button,
                Text = "Purchase",
                IsInteractable = offer.CanAccept(),
                OnClick = () =>
                {
                    var transaction = offer.Accept();
                    var result = G.Instance.TransactionProcessor.Process(transaction);
                },
                closePopupOnClick = true,
                UnavailabilityReason = "Cannot afford"
            }
        };
    }

    public static UIElement WarehouseStats(Warehouse warehouse)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"\n - Size: {warehouse.SizeCatergory}\n - Location: {warehouse.DistrictName}\n - Parking lots: {warehouse.AvailableCarParkingSpots}/{warehouse.OverallCarParkingSpots}"
        };
    }


    //Car
    public static List<UIElement> CarBaseInfoElements(Car car)
    {
        return new List<UIElement>()
        {
            Header(car.Name),
            CarIcon(car),
            CarPrice(car),
            CarStats(car)
        };
    }

    public static List<UIElement> SellCarOfferElements(CarSellOffer offer)
    {
        return new List<UIElement>()
        {
            new UIElement
            {
                Type = UIElementType.Text,
                Text = $"sell for: {offer.InitialOfferPrice}",
                Style = "price"
            },
            new UIElement
            {
                Type = UIElementType.Button,
                Text = "Sell Car",
                IsInteractable = offer.CanAccept(),
                OnClick = () =>
                {
                    var transaction = offer.Accept();
                    var result = G.Instance.TransactionProcessor.Process(transaction);
                },
                closePopupOnClick = true,
                UnavailabilityReason = "Cannot sell car"
            }
        };

    }

    public static UIElement CarIcon(Car car)
    {
        return Image(IconBuilderHelper.BuildCarSprite(car));
    }

    public static UIElement CarStats(Car car)
    {
        // placeholder
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = "- Max Speed: 180\n - Acceleration: 5.0\n - Handling: 80"
        };
    }

    public static UIElement CarPrice(Car car)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = $"{G.Economy.ProductPriceCalculator.Calculate(car)}",
            Style = "price"
        };
    }

    
}