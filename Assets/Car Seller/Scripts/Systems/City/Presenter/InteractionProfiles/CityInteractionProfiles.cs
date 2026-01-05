using System;
using UnityEngine;

/// <summary>
/// Result of evaluating a trigger in the city.
/// </summary>
public class TriggerAction : IInteractionContent
{
    public TriggerAction()
    {
        CanProceed = false;
        Action = null;
    }

    public TriggerAction(bool canProceed, Action action)
    {
        CanProceed = canProceed;
        Action = action;
    }

    public bool CanProceed { get; private set; }
    public Action Action { get; private set; }
}
