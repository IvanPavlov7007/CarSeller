using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class NormalCityContextMenuProfile : ICityContextMenuProfile
{
    public UIElement GenerateContent(object model, GameState gameState)
    {
        var neutralState = gameState as NeutralGameState;
        if (neutralState == null)
        {
            Debug.LogError($"NormalCityContextMenuProfile used with non-neutral state {gameState?.GetType().Name}");
            return null;
        }

        switch (model)
        {
            case Car car:
                return generateNormalCarContent(neutralState, car);
            case Warehouse warehouse:
                return generateNormalWarehouseContent(neutralState, warehouse);
            case CityObject cityObject:
                return generateGenericCityObjectContent(cityObject);
            default:
                Debug.LogError($"NormalCityContextMenuProfile: Unsupported model type {model.GetType()}");
                return null;
        }
    }

    private UIElement generateGenericCityObjectContent(CityObject cityObject)
    {
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>
            {
                CTX_Menu_Tools.Header(cityObject.Name),
                CTX_Menu_Tools.Description(cityObject.InfoText),
            }
        };
    }

    private UIElement generateNormalCarContent(NeutralGameState neutralGameState, Car car)
    {
        return new UIElement
        {
            Type = UIElementType.Container,
            Children = CTX_Menu_Tools
                .CarBaseInfoElements(car)
                .Concat(new[]
                {
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Steal",
                        IsInteractable = true,
                        OnClick = () => G.Instance.GameFlowManager.StealCar(car)
                    }
                }).ToList()
        };
    }

    private UIElement generateNormalWarehouseContent(NeutralGameState neutralGameState, Warehouse warehouse)
    {
        var warehouseOffer = G.Economy.WarehouseOfferProvider.GetOfferForWarehouse(warehouse);

        var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);
        if (warehouseOffer != null)
        {
            elementsList.AddRange(CTX_Menu_Tools.WarehousePurchaseElements(warehouseOffer));
        }
        else
        {
            elementsList.Add(new UIElement
            {
                Type = UIElementType.Button,
                Text = "Enter",
                OnClick = () => G.Instance.GameFlowController.EnterWarehouse(warehouse),
            });
        }

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList.ToList()
        };
    }
}

public sealed class NormalCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(object trigger, object triggerCause, GameState gameState)
    {
        Debug.LogWarning("NormalCityTriggerProfile: No trigger actions defined in normal city state");
        return new TriggerAction(false, null);
    }
}