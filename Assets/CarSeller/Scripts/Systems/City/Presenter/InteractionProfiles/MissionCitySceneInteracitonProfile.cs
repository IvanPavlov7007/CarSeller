using UnityEngine;
using static GameManager;

public sealed class MissionCityContextMenuProfile : ICityContextMenuProfile
{
    public UIElement GenerateContent(CityEntity model, GameState gameState)
    {
        var missionGameState = gameState as MissionGameState;
        if (missionGameState == null)
        {
            Debug.LogError($"{this} used with wrong state {gameState?.GetType().Name}");
            return null;
        }

        Debug.LogWarning($"{this}: No context menu defined for model type {model.Subject.GetType()}");
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

        if (ctx.Trigger is CityEntity cityEntity)
        {
            return new TriggerAction
            (
                true,
                () =>
                {
                    GameEvents.Instance.OnTargetReached?.Invoke(new CityTargetReachedEventData(cityEntity, ctx));
                }
            );
        }
        return new TriggerAction(false, null);
    }
}