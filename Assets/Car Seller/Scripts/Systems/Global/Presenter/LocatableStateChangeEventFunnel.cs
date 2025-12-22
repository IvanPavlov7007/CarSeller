using Pixelplacement;
using System;

/// <summary>
/// Funnel or hopper that listens to locatable state change related events and funnels them into a single event
/// Useful for presenters that need to respond to any locatable state change
/// Such as SceneManagers
/// </summary>
public class LocatableStateChangeEventFunnel : Singleton<LocatableStateChangeEventFunnel>
{
    private void OnEnable()
    {
        GameEvents.Instance.OnPlayerPossessionAcquired += OnLocatableAcquired;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnPlayerPossessionAcquired -= OnLocatableAcquired;
    }

    private void OnLocatableAcquired(PossesionChangeEventData data)
    {
        GameEvents.Instance.OnLocatableStateChanged?.Invoke(new LocatableStateChangedEventData(data.Possession as ILocatable));
    }

    


}