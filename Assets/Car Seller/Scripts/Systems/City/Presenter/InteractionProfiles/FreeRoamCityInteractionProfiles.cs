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
    public TriggerAction GenerateTriggerAction(object trigger, object triggerCause, GameState gameState)
    {
        if(trigger is Collectable collectable)
        {
            return new TriggerAction
            (
                true,
                () =>
                {
                    collectable.Collect();
                }
            );
        }
        return new TriggerAction(false, null);
    }
}