using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NormalCityContextMenuProfile : ICityContextMenuProfile
{
    public virtual UIElement GenerateContent(CityEntity model, GameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogError($"NormalCityContextMenuProfile used with no state");
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

            if (GameRules.carCanBeExited.Check(car))
            {
                elements.Add(new UIElement
                {
                    Type = UIElementType.Button,
                    Text = "Exit",
                    IsInteractable = true,
                    OnClick = () =>
                    {
                        G.VehicleController.ExitWorldVehicle();
                    },
                    closePopupOnClick = true
                });
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

        Debug.LogWarning($"NormalCityContextMenuProfile: No context menu defined for model type {model.Subject.GetType()}");
        return null;
    }
}

public class NormalCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    {
        var gameState = ctx.GameState;
        Debug.Assert(gameState != null, "NormalCityTriggerProfile: gameState is not set");

        // Determine who is controlled right now (figure has priority if present).
        ILocatable actor = G.VehicleController.CurrentCar;
        if (actor == null)
            return new TriggerAction(false, null);

        // Controlled entity must be the trigger cause.
        if (ctx.TriggerCause.Subject != actor)
            return new TriggerAction(false, null);


        // PlayerFigure -> Car drag end: get into car
        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is Car dragTargetCar)
        {
            return new TriggerAction(true, () =>
            {
                G.VehicleController.DriveWorldVehicle(ctx.Trigger);
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

        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is CarStashWarehouse carStashWarehouse)
        {
            return new TriggerAction(true, () => 
            { G.ProcessRunner.Run(new CarStashProcess(ctx.TriggerCause, carStashWarehouse)); });
        }

        if (ctx.Kind == TriggerContext.TriggerKind.DragEnd && ctx.Trigger.Subject is PersonalVehicleShop vehicleShop)
        {
            return new TriggerAction(true, () =>
            { G.ProcessRunner.Run(new PersonalVehicleShopProcess(vehicleShop)); });
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