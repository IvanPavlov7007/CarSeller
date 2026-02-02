using System;
using UnityEngine;

public class Triggerable : MonoBehaviour, ITriggerable
{
    public event Action<ContentProvider, ContentProvider> OnTriggerEntered;
    CustomTrigger2D CustomTrigger2D;
    ContentProvider contentProvider;

    private void Awake()
    {
        CustomTrigger2D = GetComponentInChildren<CustomTrigger2D>();
        contentProvider = GetComponent<ContentProvider>();
        if (CustomTrigger2D == null)
            Debug.LogError("No CustomTrigger2D Provided to this " + gameObject.name);
    }

    private void OnEnable()
    {
        CustomTrigger2D?.onEnter.AddListener(OnTriggered);
        InteractionController.Instance.RegisterTriggerable(this);
    }

    private void OnDisable()
    {
        InteractionController.Instance.UnregisterTriggerable(this);
        CustomTrigger2D?.onEnter.RemoveListener(OnTriggered);
    }

    private void OnTriggered(Collider2D collision)
    {
        ContentProvider triggerCause = collision.GetComponentInParent<ContentProvider>();
        Debug.Assert(triggerCause != null);
        if(triggerCause != null)
            OnTriggerEntered?.Invoke(contentProvider, triggerCause);
    }
}


public interface ITriggerable
{
    public event Action<ContentProvider, ContentProvider> OnTriggerEntered;
}