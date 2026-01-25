using UnityEngine;

public sealed class FreeRoamCityContextMenuProfile : ICityContextMenuProfile
{
    public UIElement GenerateContent(object model, GameState gameState)
    {
        var freeRoam = gameState as FreeRoamGameState;
        if (freeRoam == null)
        {
            Debug.LogError($"FreeRoamCityContextMenuProfile used with non-free-roam state {gameState?.GetType().Name}");
            return null;
        }

        // For now, re-use generic CityObject representation or show nothing.
        if(model is MissionLauncher launcher)
        {
            return CTX_Menu_Tools.MissionLauncherHint(launcher);
        }

        if (model is CityObject cityObject)
        {
            return new UIElement
            {
                Type = UIElementType.Container,
                Children = new System.Collections.Generic.List<UIElement>
                {
                    CTX_Menu_Tools.Header(cityObject.Name),
                    CTX_Menu_Tools.Description(cityObject.InfoText),
                }
            };
        }

        Debug.LogWarning($"FreeRoamCityContextMenuProfile: No context menu defined for model type {model.GetType()}");
        return null;
    }
}

public sealed class FreeRoamCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    {
        FreeRoamGameState freeRoamGameState = ctx.GameState as FreeRoamGameState;
        Debug.Assert(freeRoamGameState != null, "FreeRoamCityTriggerProfile: gameState is not FreeRoamGameState");

        if (ctx.TriggerCause != freeRoamGameState.FocusedCar)
            return new TriggerAction(false, null);
        Car car = ctx.TriggerCause as Car;
        if (ctx.Trigger is Warehouse warehouse)
        {
            // prevent instant re-entry after leaving warehouse ---
            if (!freeRoamGameState.CanEnterWarehouse(warehouse,car))
            {
                // Ignore this trigger for now
                return new TriggerAction(false, null);
            }

            return new TriggerAction
            (
                true,
                () =>
                {
                    G.CityActionService.PutCarInsideWarehouse(car, warehouse);
                    G.GameFlowController.EnterWarehouse(warehouse);
                }
            );
        }
        if(ctx.Trigger is CityObject cityObject)
        {
            return new TriggerAction
            (
                true,
                () =>
                {
                    GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityObject,ctx));
                }
            );
        }
        return new TriggerAction(false, null);
    }
}