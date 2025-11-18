using UnityEngine;
using UnityEngine.Events;

public class CustomTrigger2D : MonoBehaviour
{
    public UnityEvent<Collider2D> onEnter;
    public UnityEvent<Collider2D> onStay;
    public UnityEvent<Collider2D> onExit;

    private void Start()
    {
#if DEBUG
        if(onEnter == null)
        {
            Debug.LogWarning("No event listeners on this trigger" + ToString());
        }
#endif
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (onEnter != null)
            onEnter.Invoke(collision);
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (onStay != null)
            onStay.Invoke(collision);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (onExit != null)
            onExit.Invoke(collision);
    }

}
