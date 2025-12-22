using UnityEngine;

public interface IDirectionProvider
{
    Vector2 ProvidedDirection { get; }
}

public interface IActivatable
{
    public void Activate();
    public void Deactivate();
    public void SetActive(bool active);
}