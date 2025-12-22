using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// manager that handles logics of interactions in the city scene
/// </summary>
public class CityInteractionManager : IInteractionManager
{
    CityContextMenuContentProfile contextMenuProfile = new CityContextMenuContentProfile();
    InteractionTriggerProfile triggerProfile = new InteractionTriggerProfile();

    public void OnDragEnd(Interactable interactable)
    {
    }

    public void OnDragStart(Interactable interactable)
    {
    }

    public void OnProductViewClick(Interactable interactable)
    {
        var contentProvider = interactable.GetComponent<ContentProvider>();
        var content = contentProvider.ProvideContent(contextMenuProfile, null);
        if(content != null)
            ContextMenuManager.Instance.CreateContextMenu(interactable.gameObject, content);
    }

    public void OnTriggerEntered(ContentProvider trigger, ContentProvider triggerCause)
    {
        TriggerContext context = new TriggerContext(G.GameState, triggerCause);
        TriggerAction triggerAction = trigger.ProvideContent(triggerProfile, context);
        if(triggerAction == null)
        {
            Debug.LogError("CityInteractionManager: TriggerAction is null");
        }
        else
        {
            if(triggerAction.CanProceed)
            {
                if(triggerAction.Action == null)
                {
                    Debug.LogError("CityInteractionManager: TriggerAction.Action is null");
                    return;
                }
                triggerAction.Action.Invoke();
            }
        }
    }
}