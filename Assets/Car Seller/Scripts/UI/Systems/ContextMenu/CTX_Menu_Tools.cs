using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Utility tools for building context menu UI elements.
/// Collection of methods based on similar functionality
/// ( to have implementation in one place and avoid code duplication ).
/// </summary>
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
    public static UIElement Price(float price, bool withSign = false)
    {
        return new UIElement
        {
            Type = UIElementType.Text,
            Text = withSign? FormatPriceWithSign(price) : FormatPrice(price),
            Style = "price"
        };
    }

    static string FormatPrice(float price)
    {
        return price.ToString("C0", CultureInfo.CurrentCulture);
    }

    static string FormatPriceWithSign(float price)
    {
        string numbs = FormatPrice(Math.Abs(price));
        if(price > 0)
        {
            return $"+{numbs}";
        }
        else if(price < 0)
        {
            return $"-{numbs}";
        }
        else
        {
            return numbs;
        }
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
            Price(offer.Price),
            new UIElement
            {
                Type = UIElementType.Button,
                Text = "Purchase",
                IsInteractable = offer.CanAccept(),
                OnClick = () =>
                {
                    var transaction = offer.Accept();
                    var result = G.TransactionProcessor.Process(transaction);
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
            Text = $"\n - Size: {warehouse.SizeCategory}\n - Location: {warehouse.DistrictName}\n - Parking lots: {warehouse.AvailableCarParkingSpots}/{warehouse.OverallCarParkingSpots}",
            Style = "description"
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
            //CarStats(car)
        };
    }

    // TODO Currently not working properly - transaction should be created in the context of the car sell menu
    // TODO Make use of this in the car sell context menu
    public static List<UIElement> SellCarOfferElements(CarSellOffer offer)
    {
        return new List<UIElement>()
        {
            new UIElement
            {
                Type = UIElementType.Text,
                Text = $"sell for: {FormatPrice(offer.InitialOfferPrice)}",
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
                    var result = G.TransactionProcessor.Process(transaction);
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
            Text = "Placeholder stats:\n- Max Speed: 180\n- Acceleration: 5.0\n- Handling: 80",
            Style = "description"
        };
    }

    

    public static UIElement CarPrice(Car car)
    {
        return Price(G.Economy.ProductPriceCalculator.Calculate(car));
    }

    // Mission
    public static UIElement MissionLauncherTrigger(MissionLauncher launcher)
    {
        var mission = launcher.MissionRuntime;
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header(mission.Config.MissionId),
                Description(launcher.Config.ctxDescription),
                new UIElement
                {
                    Type = UIElementType.Button,
                    Text = "Start",
                    // TODO check that other conditions are also met
                    IsInteractable = mission.Status == MissionStatus.Available,
                    OnClick = () =>
                    {
                        GameEvents.Instance.OnPlayerAccept(new PlayerAcceptedEventData(mission));
                    },
                    closePopupOnClick = true,
                    UnavailabilityReason = "Mission cannot be started"
                }
            }
        };     
    }

    public static UIElement MissionLauncherHint(MissionLauncher launcher)
    {
        var mission = launcher.MissionRuntime;
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header(mission.Config.MissionId),
                Description(launcher.Config.ctxDescription),
                Hint("Get here, to start the mission")
            }
        };
    }

    public static UIElement MissionCompletedInfo(MissionRuntime mission)
    {
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header($"Mission {mission.Config.MissionId} Completed!"),
                new UIElement
                {
                    Type = UIElementType.Button,
                    Text = "Great!",
                    OnClick = () =>
                    {
                    },
                    closePopupOnClick = true
                }
            }
        };
    }

    internal static UIElement MissionFailedInfo(MissionRuntime mission)
    {
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
            {
                Header($"Mission {mission.Config.MissionId} Failed!"),
                new UIElement
                {
                    Type = UIElementType.Button,
                    Text = "Close",
                    OnClick = () =>
                    {
                    },
                    closePopupOnClick = true
                }
            }
        };
    }
}