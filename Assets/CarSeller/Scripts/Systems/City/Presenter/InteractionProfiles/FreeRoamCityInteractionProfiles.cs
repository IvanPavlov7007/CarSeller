using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class FreeRoamCityContextMenuProfile : ICityContextMenuProfile
{
    public UIElement GenerateContent(CityEntity model, GameState gameState)
    {
        var freeRoam = gameState as FreeRoamGameState;
        if (freeRoam == null)
        {
            Debug.LogError($"FreeRoamCityContextMenuProfile used with non-free-roam state {gameState?.GetType().Name}");
            return null;
        }

        if (model.Subject is MissionLauncher launcher)
        {
            return CTX_Menu_Tools.MissionLauncherHint(launcher);
        }

        if (model.Subject is PlayerFigure figure)
        {
            // "It's you"
            return new UIElement
            {
                Type = UIElementType.Container,
                Children = new List<UIElement>
                {
                    CTX_Menu_Tools.Header("You"),
                    CTX_Menu_Tools.Hint("It's you")
                }
            };
        }

        if (model.Subject is Car car)
        {
            List<UIElement> elements = CTX_Menu_Tools.CarBaseInfoElements(car);

            // If PlayerFigure is controlled, we can't "Drive" from context menu; show hint to get in.
            if (freeRoam.PlayerFigure != null)
            {
                elements.Add(CTX_Menu_Tools.Hint("Drag yourself onto the car to get in"));
            }
            else
            {
                bool isAlreadyFocused = car == freeRoam.FocusedCar;

                elements.Add(
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = isAlreadyFocused ? "Driving" : "Drive",
                        IsInteractable = !isAlreadyFocused,
                        UnavailabilityReason = isAlreadyFocused ? "Already driving this car" : null,
                        OnClick = () =>
                        {
                            G.GameFlowController.TryDriveCar(car, out _);
                            CameraHelper.SetCurrentPositionAtCar();
                        }
                    });

                // Exit button only for the current car: spawn/transfer control to player figure at car position.
                if (isAlreadyFocused)
                {
                    elements.Add(new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Exit",
                        IsInteractable = true,
                        OnClick = () =>
                        {
                            // Put a figure at car position and control it.
                            var pos = CityLocatorHelper.GetCityEntity(car).Position;
                            var figure = new PlayerFigure();
                            CityEntitiesCreationHelper.CreatePlayerFigure(figure, pos);
                            G.GameFlowController.TryControlPlayerFigure(figure, out _);
                        },
                        closePopupOnClick = true
                    });
                }
            }

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elements
            };
        }

        if (model.Subject is Warehouse)
        {
            // Warehouse shouldn't be enter-able by button in any case.
            // Keep only base info / purchase info.
            var warehouse = (Warehouse)model.Subject;
            var warehouseOffer = G.Economy.WarehouseOfferProvider.GetOfferForWarehouse(warehouse);

            var elementsList = CTX_Menu_Tools.WarehouseBaseInfoElements(warehouse);
            if (warehouseOffer != null)
            {
                elementsList.AddRange(CTX_Menu_Tools.WarehousePurchaseElements(warehouseOffer));
            }
            else
            {
                elementsList.Add(CTX_Menu_Tools.Hint("Drive or walk into it to enter"));
            }

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elementsList.ToList()
            };
        }

        Debug.LogWarning($"FreeRoamCityContextMenuProfile: No context menu defined for model type {model.Subject.GetType()}");
        return null;
    }
}

public sealed class FreeRoamCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    {
        var freeRoam = ctx.GameState as FreeRoamGameState;
        Debug.Assert(freeRoam != null, "FreeRoamCityTriggerProfile: gameState is not FreeRoamGameState");

        // Determine who is controlled right now (figure has priority if present).
        ILocatable actor = freeRoam.PlayerFigure != null ? (ILocatable)freeRoam.PlayerFigure : freeRoam.FocusedCar;
        if (actor == null)
            return new TriggerAction(false, null);

        // Controlled entity must be the trigger cause.
        if (ctx.TriggerCause.Subject != actor)
            return new TriggerAction(false, null);

        Debug.Log($"FreeRoamCityTriggerProfile: Triggered by {ctx.TriggerCause?.GetType().Name} on {ctx.Trigger?.GetType().Name}");

        // PlayerFigure -> Car drag end: get into car
        if (freeRoam.PlayerFigure != null && ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Car dragTargetCar)
        {
            return new TriggerAction(true, () =>
            {
                G.GameFlowController.TryDriveIntoCarFromPlayerFigure(dragTargetCar, out _);
                CameraHelper.SetCurrentPositionAtCar();
            });
        }

        // Actor -> Warehouse drag end: enter warehouse
        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Warehouse warehouse)
        {
            // Car-specific cooldown gating (figure doesn't use it)
            if (actor is Car focusedCar)
            {
                if (!G.WarehouseEntryCooldownService.CanEnterWarehouse(focusedCar, warehouse))
                    return new TriggerAction(false, null);

                return new TriggerAction(true, () => { G.ProcessRunner.Run(new WarehouseEnterProcess(focusedCar, warehouse)); });
            }

            if (actor is PlayerFigure figure)
            {
                return new TriggerAction(true, () => { G.ProcessRunner.Run(new WarehouseEnterProcess(figure, warehouse)); });
            }
        }

        // Generic city target reached events (figure should also trigger them)
        if (ctx.Trigger is CityEntity cityEntity)
        {
            return new TriggerAction(true, () =>
            {
                switch (ctx.Kind)
                {
                    case TriggerContext.TriggerKind.Enter:
                        GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
                        break;
                    case TriggerContext.TriggerKind.DragEnd:
                        GameEvents.Instance.OnTargetReachDragEnded?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
                        break;
                }
            });
        }

        return new TriggerAction(false, null);
    }
}