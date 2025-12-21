using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public abstract class CityContextMenuContentProfile : IInteractionContentProfile<UIElement>
{
    public UIElement GenerateContent(object model, IInteractionContext context)
    {
        switch (model)
        {
            case Car car:
                return generateCarContent(car);
            case Warehouse warehouse:
                return generateWarehouseContent(warehouse);
            case CityObject cityObject:
                return generateCityObjectContent(cityObject);
            default:
                Debug.LogError($"CityInteractionManager: Unsupported model type {model.GetType()}");
                return null;
        }
    }
    public virtual UIElement generateCityObjectContent(CityObject cityObject)
    {
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = cityObject.Name,
                        Style = "header"
                    },
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = cityObject.InfoText,
                    },
                }
        };
        return content;
    }

    protected abstract UIElement generateCarContent(Car car);
    protected abstract UIElement generateWarehouseContent(Warehouse warehouse);
}

public class NormalCityContextMenuContentProfile : CityContextMenuContentProfile
{

    protected override UIElement generateCarContent(Car car)
    {
        var closesWarehouse = getClosestWarehouse(car, out float distance);
        bool carNearWarehouse = distance < 0.5f;

        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Car: {car.Name}"
                    },
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Put inside warehouse",
                        IsInteractable = carNearWarehouse,
                        UnavailabilityReason = carNearWarehouse ? null : "Car is too far from warehouse",
                        OnClick = () =>
                        {
                            G.Instance.CityActionService.PutCarInsideWarehouse(car, closesWarehouse);
                        }
                    },
                }
        };

        return content;

    }

    private Warehouse getClosestWarehouse(Car car, out float distance)
    {


        var city = World.Instance.City;
        Debug.Assert(city.Locations.ContainsKey(car), "CityInteractionManager: Car position not found in city positions");
        Vector2 carPosition = city.Locations[car].CityPosition.WorldPosition;
        Dictionary<Warehouse, float> warehouseDistances = new Dictionary<Warehouse, float>();
        foreach (var obj in city.Locations.Keys)
        {
            if (obj is not Warehouse)
                continue;
            var warehousePosition = city.Locations[obj].CityPosition.WorldPosition;
            warehouseDistances.Add(obj as Warehouse, Vector2.Distance(warehousePosition, carPosition));
        }
        warehouseDistances = warehouseDistances.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        distance = warehouseDistances.First().Value;
        return warehouseDistances.First().Key;
    }
    protected override UIElement generateWarehouseContent(Warehouse warehouse)
    {
        var closestCar = getClosestCar(warehouse, out float distance);
        bool carNearWarehouse = distance < 0.5f;

        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Warehouse: {warehouse.Config.DisplayName}"
                    },
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Open",
                        OnClick = () =>
                        {
                            G.Instance.GameFlowController.EnterWarehouse(warehouse);
                        },
                    },
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Put closest car inside",
                        IsInteractable = carNearWarehouse,
                        UnavailabilityReason = carNearWarehouse ? null : "No car near warehouse",
                        OnClick = () =>
                        {
                            G.Instance.CityActionService.PutCarInsideWarehouse(closestCar, warehouse);
                        }
                    },
                }
        };
        return content;
    }

    private Car getClosestCar(Warehouse warehouse, out float distance)
    {
        var city = World.Instance.City;
        Vector2 warehousePosition = city.Locations[warehouse].CityPosition.WorldPosition;
        Dictionary<Car, float> carDistances = new Dictionary<Car, float>();
        foreach (var obj in city.Locations.Keys)
        {
            if (obj is not Car)
                continue;
            var carPosition = city.Locations[obj].CityPosition.WorldPosition;
            carDistances.Add(obj as Car, Vector2.Distance(carPosition, warehousePosition));
        }


        if (carDistances.Count == 0)
        {
            distance = float.MaxValue;
            return null;
        }
        carDistances = carDistances.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
        distance = carDistances.First().Value;
        return carDistances.First().Key;
    }
}

public class StealingCityContextMenuContentProfile : CityContextMenuContentProfile
{
    protected override UIElement generateCarContent(Car car)
    {
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Car: {car.Name}"
                    },
                }
        };
        return content;
    }
    protected override UIElement generateWarehouseContent(Warehouse warehouse)
    {
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Warehouse: {warehouse.Config.DisplayName}"
                    },
                }
        };
        return content;
    }
}

public class SellingCityContextMenuContentProfile : CityContextMenuContentProfile
{
    protected override UIElement generateCarContent(Car car)
    {
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Car: {car.Name}"
                    },
                }
        };
        return content;
    }
    protected override UIElement generateWarehouseContent(Warehouse warehouse)
    {
        UIElement content = new UIElement
        {
            Type = UIElementType.Container,
            Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Warehouse: {warehouse.Config.DisplayName}"
                    },
                }
        };
        return content;
    }
}



public abstract class InteractionTriggerProfile
{
    public abstract bool CanProceed(GameState gameState, ContentProvider trigger, ContentProvider triggerCause);
    public override void Execute(GameState gameState, ContentProvider trigger, ContentProvider triggerCause);
}

public class NormalCityInteractionTriggerProfile : InteractionTriggerProfile
{
    public override bool CanProceed(GameState gameState, ContentProvider trigger, ContentProvider triggerCause)
    {
        Debug.LogWarning("NormalCityInteractionTriggerProfile: No trigger actions defined in normal city state");
        return false;
    }
    public override void Execute(GameState gameState, ContentProvider trigger, ContentProvider triggerCause)
    {
        // No-op
    }
}

public class StealingCityInteractionTriggerProfile : InteractionTriggerProfile
{
    public override bool CanProceed(GameState gameState, ContentProvider trigger, ContentProvider triggerCause)
    {
        Debug.Assert(gameState is StealingGameState);

        var stealingData = gameState as StealingGameState;
        var warehouse = trigger.Model as Warehouse;

        return warehouse != null && warehouse.AvailableCarParkingSpots > 0 && (triggerCause.Model as Car == stealingData.StealingCar);
        
    }

    public override void Execute(GameState gameState, ContentProvider trigger, ContentProvider triggerCause)
    {
        var stealingState = gameState as StealingGameState;
        GameEvents.Instance.OnPlayerSucceed(new PlayerActionEventData(PlayerOutcome.Succeeded, trigger.Model as Warehouse));
    }
}

public class SellingCityInteractionTriggerProfile : InteractionTriggerProfile
{
    public override bool CanProceed(GameState gameState, ContentProvider trigger, ContentProvider triggerCause)
    {
        var sellingState = gameState as SellingGameState;

        return (trigger.Model as Buyer == sellingState.Buyer) && (triggerCause.Model as Car == sellingState.SellingCar);

    }
    public override void Execute(GameState gameState, ContentProvider trigger, ContentProvider triggerCause)
    {
        GameEvents.Instance.OnPlayerSucceed(new PlayerActionEventData(PlayerOutcome.Succeeded, null));
    }
}


public sealed class CityInteractionProfileRegistry
{
    private readonly Dictionary<Type, CityContextMenuContentProfile> _ContextMenuProfiles;
    private readonly Dictionary<Type, InteractionTriggerProfile> _TriggerProfiles;

    public CityInteractionProfileRegistry()
    {
        _ContextMenuProfiles = new()
        {
            { typeof(NeutralGameState), new NormalCityContextMenuContentProfile() },
            { typeof(StealingGameState), new StealingCityContextMenuContentProfile() },
            { typeof(SellingGameState), new SellingCityContextMenuContentProfile() },
        };
        _TriggerProfiles = new()
        {
            { typeof(NeutralGameState), new NormalCityInteractionTriggerProfile() },
            { typeof(StealingGameState), new StealingCityInteractionTriggerProfile() },
            { typeof(SellingGameState), new SellingCityInteractionTriggerProfile() },
        };
    }

    public CityContextMenuContentProfile GetContextMenuProfile(GameState state)
        => _ContextMenuProfiles[state.GetType()];

    public InteractionTriggerProfile GetTriggerProfile(GameState state)
        => _TriggerProfiles[state.GetType()];
}