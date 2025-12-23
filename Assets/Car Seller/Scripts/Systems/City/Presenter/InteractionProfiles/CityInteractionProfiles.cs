using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ContextMenuContext : IInteractionContext
{
    public ContextMenuContext(GameState gameState)
    {
        GameState = gameState;
    }
    public GameState GameState;
}

public class CityContextMenuContentProfile : IInteractionContentProfile<UIElement, ContextMenuContext>
{
    public UIElement GenerateContent(object model, ContextMenuContext context)
    {
        Debug.Assert(context != null);
        Debug.Assert(model != null);

        switch (context.GameState)
        {
            case NeutralGameState normalState:
                return generateNormalContent(normalState, model);
            case StealingGameState stealingState:
                return generateStealingContent(stealingState, model);
            case SellingGameState sellingState:
                return generateSellingContent(sellingState, model);
            default:
                Debug.LogError("Unknown context for generating content");
                return null;
        }
    }
    // Content-independent
    private UIElement generateGenericCityObjectContent(CityObject cityObject)
    {
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    CTX_Menu_Tools.Header(cityObject.Name),
                    CTX_Menu_Tools.Description(cityObject.InfoText),
                }
        };
        return content;
    }

    // Neutral
    private UIElement generateNormalContent(NeutralGameState neutralGameState, object model)
    {
        switch (model)
        {
            case Car car:
                return generateNormalCarContent(neutralGameState, car);
            case Warehouse warehouse:
                return generateNormalWarehouseContent(neutralGameState, warehouse);
            case CityObject cityObject:
                return generateGenericCityObjectContent(cityObject);
            default:
                Debug.LogError($"CityInteractionManager: Unsupported model type {model.GetType()}");
                return null;
        }
    }
    private UIElement generateNormalCarContent(NeutralGameState neutralGameState,Car car)
    {
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = CTX_Menu_Tools.CarBaseInfoElements(car).Concat(
            new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Steal",
                        IsInteractable = true,
                        OnClick = () =>
                        {
                            G.Instance.GameFlowManager.StealCar(car);
                        }
                    },
                }
            ).ToList()
        };
        return content;
    }
    private UIElement generateNormalWarehouseContent(NeutralGameState neutralGameState,Warehouse warehouse)
    {
        var warehouseOffer = G.Economy.WarehouseOfferProvider.GetOfferForWarehouse(warehouse);

        // Build children list
        var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);
        if(warehouseOffer != null)
        {
            elementsList.AddRange(CTX_Menu_Tools.WarehousePurchaseElements(warehouseOffer));
        }
        else // No offer, already owned
        {
            elementsList.Add(new UIElement
            {
                Type = UIElementType.Button,
                Text = "Enter",
                OnClick = () =>
                {
                    G.Instance.GameFlowController.EnterWarehouse(warehouse);
                },
            });
        }


            UIElement content = new UIElement
            {
                Type = UIElementType.Container,
                Children = elementsList.ToList()
            };
        return content;
    }

    // Stealing
    private UIElement generateStealingContent(StealingGameState stealingGameState, object model)
    {
        switch (model)
        {
            case Car car:
                return generateStealingCarContent(stealingGameState,car);
            case Warehouse warehouse:
                return generateStealingWarehouseContent(stealingGameState,warehouse);
            case CityObject cityObject:
                return generateGenericCityObjectContent(cityObject);
            default:
                Debug.LogError($"CityInteractionManager: Unsupported model type {model.GetType()}");
                return null;
        }
    }
    private UIElement generateStealingCarContent(StealingGameState stealingGameState, Car car)
    {
        var elementsList = CTX_Menu_Tools.CarBaseInfoElements(car);
        if(stealingGameState.StealingCar == car)
        {
            elementsList.Add(CTX_Menu_Tools.Hint("This is the car you are currently stealing. Find a warehouse to store it in."));
            elementsList.Add(CTX_Menu_Tools.CancelButton());
        }
        else
        {
            elementsList.Add(CTX_Menu_Tools.Hint("You are already stealing another car. Find a warehouse to store it in before stealing another."));
        }

        var content = new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList.ToList()
        };
        return content;
    }
    private UIElement generateStealingWarehouseContent(StealingGameState stealingGameState, Warehouse warehouse)
    {
        var warehouseOffer = G.Economy.WarehouseOfferProvider.GetOfferForWarehouse(warehouse);

        // Build children list
        var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);
        if (warehouseOffer != null)
        {
            elementsList.AddRange(CTX_Menu_Tools.WarehousePurchaseElements(warehouseOffer));
        }
        else // No offer, already owned
        {
            elementsList.Add(
                CTX_Menu_Tools.Hint(warehouse.AvailableCarParkingSpots > 0 ? 
                "Ride into this warehouse to complete your car-theft" : 
                "Not enough space to store an additional auto"));
        }

        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList.ToList()
        };
        return content;
    }

    // Selling
    private UIElement generateSellingContent(SellingGameState sellingState, object model)
    {
        switch (model)
        {
            case Car car:
                return generateSellingCarContent(sellingState, car);
            case Warehouse warehouse:
                return generateSellingWarehouseContent(sellingState, warehouse);
            case CityObject cityObject:
                return generateGenericCityObjectContent(cityObject);
            default:
                Debug.LogError($"CityInteractionManager: Unsupported model type {model.GetType()}");
                return null;
        }
    }
    private UIElement generateSellingWarehouseContent(SellingGameState sellingState, Warehouse warehouse)
    {
        // Build children list
        var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList.ToList()
        };
        return content;
    }
    private UIElement generateSellingCarContent(SellingGameState sellingState, Car car)
    {
        var elementsList = CTX_Menu_Tools.CarBaseInfoElements(car);
        elementsList.Add(CTX_Menu_Tools.Hint("This is the car you are currently selling. Bring it to the buyer."));
        elementsList.Add(CTX_Menu_Tools.CancelButton());

        var  content = new UIElement
        {
            Type = UIElementType.Container,
            Children = elementsList.ToList()
        };
        return content;
    }
}


public class TriggerContext : IInteractionContext
{
    public readonly GameState GameState;
    public readonly ContentProvider TriggerCause;
    public TriggerContext(GameState gameState, ContentProvider triggerCause)
    {
        GameState = gameState;
        TriggerCause = triggerCause;
    }
}

public class TriggerAction : IInteractionContent
{
    public TriggerAction()
    {
        CanProceed = false;
        Action = null;
    }

    public TriggerAction(bool canProceed, Action action)
    {
        CanProceed = canProceed;
        Action = action;
    }

    public bool CanProceed { get; private set; } = false;
    public Action Action { get; private set; }
}

public class InteractionTriggerProfile : IInteractionContentProfile<TriggerAction, TriggerContext>
{
    public TriggerAction GenerateContent(object model, TriggerContext context)
    {
        switch (context.GameState)
        {
            case NeutralGameState normalState:
                return generateNormalTriggerAction(normalState, model, context.TriggerCause.Model);
            case StealingGameState stealingState:
                return generateStealingTriggerAction(stealingState,model,context.TriggerCause.Model);
            case SellingGameState sellingState:
                return generateSellingTriggerAction(sellingState, model, context.TriggerCause.Model);
            default:
                Debug.LogWarning($"CityInteractionManager: Unsupported game state type {context.GameState.GetType()}");
                return new TriggerAction();
        }
    }

    private TriggerAction generateSellingTriggerAction(SellingGameState sellingState, object trigger, object triggerCause)
    {
        Debug.Assert(sellingState.SellingCar != null, "SellingCityInteractionTriggerProfile: SellingCar is null");
        Debug.Assert(sellingState.Buyer != null, "SellingCityInteractionTriggerProfile: Buyer is null");

        return new TriggerAction(
            (trigger as Buyer == sellingState.Buyer) && (triggerCause as Car == sellingState.SellingCar),
            () => GameEvents.Instance.OnPlayerSucceed(new PlayerActionEventData(PlayerOutcome.Succeeded, null))
            );
    }

    public TriggerAction generateNormalTriggerAction(NeutralGameState neutralState, object trigger, object triggerCause)
    {
        Debug.LogWarning("InteractionTriggerProfile: No trigger actions defined in normal city state");
        return new TriggerAction();
    }

    public TriggerAction generateStealingTriggerAction(StealingGameState stealingState, object trigger, object triggerCause)
    {
        bool canProceed = false;
        Action triggerAction = null;

        var warehouse = trigger as Warehouse;
        Debug.Assert(warehouse != null, "StealingCityInteractionTriggerProfile: trigger is not Warehouse");
        Debug.Assert(stealingState.StealingCar != null, "StealingCityInteractionTriggerProfile: StealingCar is null");

        canProceed = warehouse.AvailableCarParkingSpots > 0 && (triggerCause as Car == stealingState.StealingCar);
        triggerAction = () =>
        {
            GameEvents.Instance.OnPlayerSucceed(new PlayerActionEventData(PlayerOutcome.Succeeded, warehouse));
        };
        return new TriggerAction(canProceed, triggerAction);
    }
}
