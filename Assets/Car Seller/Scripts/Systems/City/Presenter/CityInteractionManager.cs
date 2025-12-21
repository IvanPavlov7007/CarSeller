using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StealingCityInteractionTriggerProfile;

public class CityInteractionManager : IInteractionManager
{
    CityInteractionProfileRegistry interactionProfileRegistry = new CityInteractionProfileRegistry();

    CityContextMenuContentProfile currentContextMenuProfile;
     InteractionTriggerProfile currentTriggerProfile;


    public void OnDragEnd(Interactable interactable)
    {
    }

    public void OnDragStart(Interactable interactable)
    {
    }

    public void OnProductViewClick(Interactable interactable)
    {
        var contentProvider = interactable.GetComponent<ContentProvider>();
        var content = contentProvider.ProvideContent(currentContextMenuProfile, null);
        if(content != null)
            ContextMenuManager.Instance.CreateContextMenu(interactable.gameObject, content);
    }

    public void OnTriggerEntered(ContentProvider trigger, ContentProvider triggerCause)
    {
        if (currentTriggerProfile.CanProceed(G.GameState, trigger, triggerCause))
            currentTriggerProfile.Execute(G.GameState, trigger, triggerCause);
    }

    public void OnGameStateChanged(GameStateChangeEventData data)
    {
        currentContextMenuProfile = interactionProfileRegistry.GetContextMenuProfile(data.newState);
        currentTriggerProfile = interactionProfileRegistry.GetTriggerProfile(data.newState);
    }
}