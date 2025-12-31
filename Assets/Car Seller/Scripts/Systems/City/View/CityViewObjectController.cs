using System;
using UnityEngine;

public enum ViewObjectVisualState
{
    Normal,
    Disabled,
}

public class CityViewObjectController : MonoBehaviour
{
    public ILocatable Locatable { get; private set; }

    public event Action<ViewObjectVisualState> OnVisualStateChanged;
    public event Action<bool> OnDraggableStateChanged;
    public event Action OnDestroyed;

    public ViewObjectVisualState CurrentVisualState { get; private set; }
    public bool IsDraggable { get; private set; }

    public CityViewObjectController Initialize(ILocatable locatable, ViewObjectVisualState cityObjectVisualState = ViewObjectVisualState.Normal, bool isDraggable = false)
    {
        Locatable = locatable;
        IsDraggable = isDraggable;
        CurrentVisualState = cityObjectVisualState;
        return this;
    }

    public void SetViewState(CityObjectState cityObjectState)
    {
        SetVisibility(cityObjectState.visualState);
        SetDraggable(cityObjectState.draggable);
    }

    public void SetVisibility(ViewObjectVisualState visualState)
    {
        switch (visualState)
        {
            case ViewObjectVisualState.Normal:
                break;
            case ViewObjectVisualState.Disabled:
                break;
            default:
                Debug.LogError($"CityViewObject: Unsupported visual state {visualState}");
                return;
        }
        CurrentVisualState = visualState;
        OnVisualStateChanged?.Invoke(visualState);
    }

    public void SetDraggable(bool draggable)
    {
        IsDraggable = draggable;
        OnDraggableStateChanged?.Invoke(draggable);
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}