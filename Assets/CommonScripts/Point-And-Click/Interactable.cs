using System;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public event Action<Interactable> CursorEntered;
    public event Action<Interactable> CursorExited;
    public event Action<Interactable> CursorSelectStarted;
    public event Action<Interactable> CursorSelectEnded;
    public event Action<Interactable> CursorDragStarted;
    public event Action<Interactable> CursorDragEnded;

    protected bool CursorHovering { get; private set; }

    public void CursorSelectStart()
    {
        CursorSelectStarted?.Invoke(this);
        OnCursorSelectStart();
    }

    public void CursorSelectEnd()
    {
        CursorSelectEnded?.Invoke(this);
        OnCursorSelectEnd();
    }

    public void CursorEnter()
    {
        CursorEntered?.Invoke(this);
        CursorHovering = true;
        OnCursorEnter();
    }

    public void CursorExit()
    {
        CursorExited?.Invoke(this);
        CursorHovering = false;
        OnCursorExit();
    }

    public void DragStart()
    {
        CursorDragStarted?.Invoke(this);
        OnCursorDragStart();
    }

    public void DragEnd()
    {
        CursorDragEnded?.Invoke(this);
        OnCursorDragEnd();
    }

    public void Drag()
    {
        OnCursorDrag();
    }

    protected virtual void OnCursorEnter() { }
    protected virtual void OnCursorStay() { }
    protected virtual void OnCursorExit() { }
    protected virtual void OnCursorSelectEnd() { }
    protected virtual void OnCursorSelectStart() { }
    protected virtual void OnCursorDrag() { }
    protected virtual void OnCursorDragStart() { }
    protected virtual void OnCursorDragEnd() { }

}
