using UnityEngine;

/// <summary>
/// Manager that handles logic of interactions in the city scene,
/// using registry-based profiles per GameState (similar to CitySceneManager).
/// </summary>
public class CityInteractionManager : IInteractionManager
{
    readonly CityContextMenuProfileRegistry _contextMenuRegistry = new CityContextMenuProfileRegistry();
    readonly CityTriggerProfileRegistry _triggerRegistry = new CityTriggerProfileRegistry();

    public void OnDragEnd(Interactable interactable)
    {
        // Not used in city at the moment.
    }

    public void OnDragStart(Interactable interactable)
    {
        // Not used in city at the moment.
    }

    public void OnProductViewClick(Interactable interactable)
    {
        var gameState = G.GameState;
        var profile = _contextMenuRegistry.Get(gameState);
        if (profile == null)
        {
            Debug.LogError("CityInteractionManager: No context menu profile available for current game state");
            return;
        }

        var contentProvider = interactable.GetComponent<ContentProvider>();
        if (contentProvider == null)
        {
            Debug.LogError("CityInteractionManager: Interactable has no ContentProvider, cannot build menu");
            return;
        }

        var model = contentProvider.Model;
        if (model == null)
        {
            Debug.LogError("CityInteractionManager: ContentProvider.Model is null");
            return;
        }

        var content = profile.GenerateContent(model, gameState);
        if (content != null)
        {
            ContextMenuManager.Instance.CreateContextMenu(interactable.gameObject, content);
        }
    }

    public void OnTriggerEntered(ContentProvider trigger, ContentProvider triggerCause)
    {
        var gameState = G.GameState;
        var profile = _triggerRegistry.Get(gameState);
        if (profile == null)
        {
            Debug.LogError("CityInteractionManager: No trigger profile available for current game state");
            return;
        }

        if (trigger == null)
        {
            Debug.LogError("CityInteractionManager: trigger is null");
            return;
        }

        var triggerModel = trigger.Model;
        var causeModel = triggerCause != null ? triggerCause.Model : null;

        var triggerAction = profile.GenerateTriggerAction(
            new TriggerContext(triggerModel, causeModel, gameState, trigger.gameObject,triggerCause.gameObject));
        if (triggerAction == null)
        {
            Debug.LogError("CityInteractionManager: TriggerAction is null");
            return;
        }

        if (!triggerAction.CanProceed)
            return;

        if (triggerAction.Action == null)
        {
            Debug.LogError("CityInteractionManager: TriggerAction.Action is null");
            return;
        }

        triggerAction.Action.Invoke();
    }
}