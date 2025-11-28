using Pixelplacement;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lives in the scene and manages all interactable objects
/// </summary>
public class InteractionController : Singleton<InteractionController>
{
    public List<Interactable> interactables = new List<Interactable>();

    IInteractionManager interactionManager => G.Instance.InteractionManager;
    
    public void RegisterInteractable(Interactable interactable)
    {
        if (!interactables.Contains(interactable))
        {
            interactables.Add(interactable);

            interactable.CursorClicked += OnClick;
            interactable.CursorDragStarted += OnDragStart;
            interactable.CursorDragEnded += OnDragEnd;
        }
    }

    public void UnregisterInteractable(Interactable interactable)
    {
        if (interactables.Contains(interactable))
        {
            interactables.Remove(interactable);
            interactable.CursorClicked -= OnClick;
            interactable.CursorDragStarted -= OnDragStart;
            interactable.CursorDragEnded -= OnDragEnd;
        }
    }

    void OnClick(Interactable interactable)
    {
        interactionManager.OnProductViewClick(interactable);
    }

    void OnDragStart(Interactable interactable)
    {
        interactionManager.OnDragStart(interactable);
    }

    void OnDragEnd(Interactable interactable)
    {
        interactionManager.OnDragEnd(interactable); 
    }

}
