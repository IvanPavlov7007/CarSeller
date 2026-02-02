using UnityEngine;
using static GameManager;

public sealed class MissionCityContextMenuProfile : ICityContextMenuProfile
{
    public UIElement GenerateContent(object model, GameState gameState)
    {
        var missionGameState = gameState as MissionGameState;
        if (missionGameState == null)
        {
            Debug.LogError($"{this} used with wrong state {gameState?.GetType().Name}");
            return null;
        }

        // For now, re-use generic CityObject representation or show nothing.
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

        Debug.LogWarning($"{this}: No context menu defined for model type {model.GetType()}");
        return null;
    }
}

public sealed class MissionCityTriggerProfile : ICityTriggerProfile
{
    public TriggerAction GenerateTriggerAction(TriggerContext ctx)
    {
        var missionGameState = ctx.GameState as MissionGameState;
        if (missionGameState == null)
        {
            Debug.LogError($"{this} used with wrong {ctx.GameState?.GetType().Name}");
            return new TriggerAction(false, null);
        }
        if (ctx.TriggerCause != missionGameState.FocusedCar)
            return new TriggerAction(false, null);
        Car car = ctx.TriggerCause as Car;

        if (ctx.Trigger is CityObject cityObject)
        {
            return new TriggerAction
            (
                true,
                () =>
                {
                    GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityObject, ctx));
                }
            );
        }
        return new TriggerAction(false, null);
    }
}