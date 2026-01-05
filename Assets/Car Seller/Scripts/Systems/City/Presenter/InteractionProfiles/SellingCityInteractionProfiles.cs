using System.Collections.Generic;
using UnityEngine;

public sealed class SellingCityContextMenuProfile : ICityContextMenuProfile
{
    public UIElement GenerateContent(object model, GameState gameState)
    {
        var sellingState = gameState as SellingGameState;
        if (sellingState == null)
        {
            Debug.LogError($"SellingCityContextMenuProfile used with non-selling state {gameState?.GetType().Name}");
            return null;
        }

        switch (model)
        {
            case Car car:
                return generateSellingCarContent(sellingState, car);
            case Warehouse warehouse:
                return generateSellingWarehouseContent(sellingState, warehouse);
            case CityObject cityObject:
                return generateGenericCityObjectContent(cityObject);
            default:
                Debug.LogError($"SellingCityContextMenuProfile: Unsupported model type {model.GetType()}");
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

    private UIElement generateSellingWarehouseContent(SellingGameState sellingState, Warehouse warehouse)
    {
        var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList
        };
    }

    private UIElement generateSellingCarContent(SellingGameState sellingState, Car car)
    {
        var elementsList = CTX_Menu_Tools.CarBaseInfoElements(car);
        elementsList.Add(CTX_Menu_Tools.Hint("This is the car you are currently selling. Bring it to the buyer."));
        elementsList.Add(CTX_Menu_Tools.CancelButton());

        return new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList
        };
    }
}

public sealed class SellingCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(object trigger, object triggerCause, GameState gameState)
    {
        var sellingState = gameState as SellingGameState;
        if (sellingState == null)
        {
            Debug.LogError($"SellingCityTriggerProfile used with non-selling state {gameState?.GetType().Name}");
            return new TriggerAction(false, null);
        }

        Debug.Assert(sellingState.SellingCar != null, "SellingCityTriggerProfile: SellingCar is null");
        Debug.Assert(sellingState.Buyer != null, "SellingCityTriggerProfile: Buyer is null");

        bool canProceed =
            trigger as Buyer == sellingState.Buyer &&
            triggerCause as Car == sellingState.SellingCar;

        System.Action action = null;
        if (canProceed)
        {
            action = () =>
            {
                GameEvents.Instance.OnPlayerSucceed(
                    new PlayerActionEventData(PlayerOutcome.Succeeded, null));
            };
        }

        return new TriggerAction(canProceed, action);
    }
}