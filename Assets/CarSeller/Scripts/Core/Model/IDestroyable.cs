using System;

/// <summary>
/// Should be used only by internal systems to notify when an object is being destroyed.
/// </summary>
public interface IDestroyable
{
    event Action<IDestroyable> onBeingDestroyed;
    void Destroy();
}