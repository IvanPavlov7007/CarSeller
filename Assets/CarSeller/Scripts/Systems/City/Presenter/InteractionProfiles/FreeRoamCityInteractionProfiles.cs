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
        if(model.Subject is MissionLauncher launcher)
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

        if (ctx.TriggerCause.Subject != freeRoamGameState.FocusedCar)
            return new TriggerAction(false, null);
        Car car = ctx.TriggerCause.Subject as Car;
        if (ctx.Trigger.Subject is Warehouse warehouse)
        {
            if (!G.WarehouseEntryCooldownService.CanEnterWarehouse(car,warehouse))
                return new TriggerAction(false, null);

            return new TriggerAction
            (
                true,
                () => { G.ProcessRunner.Run(new WarehouseEnterProcess(car, warehouse)); }

                // for migration of CarShop
                //{
                //    G.CityActionService.PutCarInsideWarehouse(car, warehouse);
                //    G.GameFlowController.EnterWarehouse(warehouse);
                //}
            );
        }
        if(ctx.Trigger is CityEntity cityEntity)
        {
            return new TriggerAction
            (
                true,
                () =>
                {
                    GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityEntity,ctx));
                }
            );
        }
        return new TriggerAction(false, null);
    }
}