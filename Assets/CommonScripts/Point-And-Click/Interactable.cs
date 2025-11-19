using System;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public int sortingOrder = 0;

    public event Action<Interactable> CursorEntered;
    public event Action<Interactable> CursorExited;
    public event Action<Interactable> CursorSelectStarted;
    public event Action<Interactable> CursorSelectEnded;
    public event Action<Interactable> CursorDragStarted;
    public event Action<Interactable> CursorDragEnded;

    // New: higher-level gestures
    public event Action<Interactable> CursorClicked;
    public event Action<Interactable> CursorHoldStarted;
    public event Action<Interactable> CursorHoldEnded;

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

    // New: Click + Hold APIs
    public void CursorClick()
    {
        CursorClicked?.Invoke(this);
        OnCursorClick();
    }

    public void CursorHoldStart()
    {
        CursorHoldStarted?.Invoke(this);
        OnCursorHoldStart();
    }

    public void CursorHold()
    {
        OnCursorHold();
    }

    public void CursorHoldEnd()
    {
        CursorHoldEnded?.Invoke(this);
        OnCursorHoldEnd();
    }

    protected virtual void OnCursorEnter()
    {
        Debug.Log("OnCursorEnter");
    }
    protected virtual void OnCursorStay() 
    {
        Debug.Log("OnCursorStay");
    }
    protected virtual void OnCursorExit() 
    {
        Debug.Log("OnCursorExit");
    }
    protected virtual void OnCursorSelectEnd() 
    {
        Debug.Log("OnCursorSelectEnd");
    }
    protected virtual void OnCursorSelectStart() 
    {
        Debug.Log("OnCursorSelectStart");
    }
    protected virtual void OnCursorDrag() 
    {
        Debug.Log("OnCursorDrag");
    }
    protected virtual void OnCursorDragStart() 
    {
        Debug.Log("OnCursorDragStart");
    }
    protected virtual void OnCursorDragEnd() 
    {
        Debug.Log("OnCursorDragEnd");
    }

    // New virtuals
    protected virtual void OnCursorClick() 
    {
        Debug.Log("OnCursorClick");
    }
    protected virtual void OnCursorHoldStart() 
    {
        Debug.Log("OnCursorHoldStart");
    }
    protected virtual void OnCursorHold() 
    {
        Debug.Log("OnCursorHold");
    }
    protected virtual void OnCursorHoldEnd() 
    {
        Debug.Log("OnCursorHoldEnd");
    }
}