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

        // For now, re-use generic CityObject representation or show nothing.
        if (model.Subject is MissionLauncher launcher)
        {
            return CTX_Menu_Tools.MissionLauncherHint(launcher);
        }

        if (model.Subject is Car car)
        {
            List<UIElement> elements = CTX_Menu_Tools.CarBaseInfoElements(car);

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

            return new UIElement
            {
                Type = UIElementType.Container,
                Children = elements
            };
        }

        if (model.Subject is Warehouse warehouse)
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
                    OnClick = () => G.GameFlowController.EnterWarehouse(warehouse),
                });
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
        FreeRoamGameState freeRoamGameState = ctx.GameState as FreeRoamGameState;
        Debug.Assert(freeRoamGameState != null, "FreeRoamCityTriggerProfile: gameState is not FreeRoamGameState");

        Debug.Log($"FreeRoamCityTriggerProfile: Triggered by {ctx.TriggerCause?.GetType().Name} on {ctx.Trigger?.GetType().Name}");

        // PlayerFigure flow: if a player figure is controlled, it is the only thing that can trigger city actions.
        if (freeRoamGameState.PlayerFigure != null)
        {
            if (ctx.TriggerCause.Subject != freeRoamGameState.PlayerFigure)
                return new TriggerAction(false, null);

            if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Car car)
            {
                return new TriggerAction(true, () =>
                {
                    G.GameFlowController.TryDriveIntoCarFromPlayerFigure(car, out _);
                    CameraHelper.SetCurrentPositionAtCar();
                });
            }

            return new TriggerAction(false, null);
        }

        // Car flow (existing)
        if (ctx.TriggerCause.Subject != freeRoamGameState.FocusedCar)
            return new TriggerAction(false, null);
        Car focusedCar = ctx.TriggerCause.Subject as Car;

        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Warehouse warehouse)
        {
            if (!G.WarehouseEntryCooldownService.CanEnterWarehouse(focusedCar, warehouse))
                return new TriggerAction(false, null);

            return new TriggerAction
            (
                true,
                () => { G.ProcessRunner.Run(new WarehouseEnterProcess(focusedCar, warehouse)); }
            );
        }
        if (ctx.Trigger is CityEntity cityEntity)
        {
            return new TriggerAction
            (
                true,
                () =>
                {
                    switch (ctx.Kind)
                    {
                        case TriggerContext.TriggerKind.Enter:
                            GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
                            break;
                        case TriggerContext.TriggerKind.DragEnd:
                            GameEvents.Instance.OnTargetReachDragEnded?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
                            break;
                        default:
                            break;
                    }

                }
            );
        }
        return new TriggerAction(false, null);
    }
}