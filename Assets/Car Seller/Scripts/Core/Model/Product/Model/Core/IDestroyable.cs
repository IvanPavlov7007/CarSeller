using System;

/// <summary>
/// Should be used only by internal systems to notify when an object is being destroyed.
/// </summary>
internal interface IDestroyable
{
    event Action<IDestroyable> onBeingDestroyed;
    void Destroy();
}