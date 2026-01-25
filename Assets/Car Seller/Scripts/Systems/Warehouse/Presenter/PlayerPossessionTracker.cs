using Pixelplacement;
using System;
using UnityEngine;

//TODO in the future, make the Ownership system as suggested by GPT
// - ownership based on containment graph


/// <summary>
/// Unfinished system to track player possessions based on locatable events. Please apply suggestion above.
/// </summary>
public class PlayerPossessionTracker : Singleton<PlayerPossessionTracker>
{
    private void OnEnable()
    {
        GameEvents.Instance.OnLocatableCreated += onLocatableCreated;
        GameEvents.Instance.OnLocatableDestroyed += onLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged += onLocatableLocationChanged;
        // Never put GameEvents.Instance.OnLocatableStateChanged
        // here, because that would cause infinite loop
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnLocatableCreated -= onLocatableCreated;
        GameEvents.Instance.OnLocatableDestroyed -= onLocatableDestroyed;
        GameEvents.Instance.OnLocatableLocationChanged -= onLocatableLocationChanged;
    }

    private void onLocatableLocationChanged(LocatableLocationChangedEventData data)
    {

    }

    private void onLocatableDestroyed(LocatableDestroyedEventData data)
    {
    }

    private void onLocatableCreated(LocatableCreatedEventData data)
    {
        Debug.Assert(data.Locatable != null, "LocatableCreatedEventData has null Locatable.");
        Debug.Assert(data.Location != null, "LocatableCreatedEventData has null Location.");
        if (isPlayerOwner(data.Location))
        {
            G.PlayerManager.AddPossession(data.Locatable as IPossession);
        }
    }

    private bool isPlayerOwner(ILocation location)
    {
        return G.Player.Owns(location as IPossession);
    }
}