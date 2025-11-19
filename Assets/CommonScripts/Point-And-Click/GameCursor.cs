using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using UnityEngine.InputSystem;

public class GameCursor : Singleton<GameCursor>
{
    public Interactable currentInteractable { get; private set; }
    public Interactable draggedInteractable { get; private set; }

    [SerializeField]
    private LayerMask interactableLayers;

    public bool use3dPhysics = false;

    // Gesture thresholds (pixels/time)
    [Header("Gesture thresholds")]
    [Tooltip("Minimum screen-space movement to start dragging.")]
    [SerializeField] private float dragStartDistancePixels = 10f;
    [Tooltip("How long to press (no movement) before Hold begins.")]
    [SerializeField] private float holdStartTime = 0.35f;
    [Tooltip("Allowed small jitter while evaluating Hold.")]
    [SerializeField] private float holdMoveTolerancePixels = 6f;
    [Tooltip("Max duration to still count as Click when released (if not dragged or held).")]
    [SerializeField] private float clickMaxTime = 0.30f;

    private bool dragStarted = false;

    // Pointer state
    private Vector2 lastScreenPosition;
    private bool pointerWasPressed;
    private bool isPointerDown;

    // Gesture state
    private Vector2 pressScreenPosition;
    private float pressTimeUnscaled;
    private Interactable pressInteractable;
    private bool holdActive;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        HandlePointerInput();
        updateWorldPosition();      // keep transform synced
        processDrag();
        processHold();
    }

    private void HandlePointerInput()
    {
        bool pointerPressed;
        Vector2 pointerPosition;

        // Prefer touch when present
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch = Touchscreen.current.primaryTouch;
            pointerPosition = touch.position.ReadValue();
            pointerPressed = touch.press.isPressed;
        }
        else if (Mouse.current != null)
        {
            pointerPosition = Mouse.current.position.ReadValue();
            pointerPressed = Mouse.current.leftButton.isPressed;
        }
        else
        {
            // No pointer device available
            return;
        }

        lastScreenPosition = pointerPosition;

        // Update hover / current interactable each frame (even during press/drag, tweak if needed)
        Vector2 worldPosition = ScreenToWorld(lastScreenPosition);
        currentInteractable = updateCurrentInteractable(currentInteractable, worldPosition, interactableLayers);

        // Press start
        if (pointerPressed && !pointerWasPressed)
        {
            isPointerDown = true;
            pressScreenPosition = lastScreenPosition;
            pressTimeUnscaled = Time.unscaledTime;
            pressInteractable = currentInteractable;
            dragStarted = false;
            draggedInteractable = null;
            holdActive = false;

            if (pressInteractable != null)
            {
                pressInteractable.CursorSelectStart();
            }
        }

        // While pressed: evaluate drag/hold thresholds
        if (pointerPressed && pointerWasPressed)
        {
            if (pressInteractable != null)
            {
                float heldTime = Time.unscaledTime - pressTimeUnscaled;
                float movedPixels = Vector2.Distance(lastScreenPosition, pressScreenPosition);

                // If we were holding and movement now exceeds drag threshold, switch to drag
                if (!dragStarted && movedPixels >= dragStartDistancePixels)
                {
                    if (holdActive)
                    {
                        pressInteractable.CursorHoldEnd();
                        holdActive = false;
                    }

                    draggedInteractable = pressInteractable;
                    dragStarted = true;
                    // DragStart happens in processDrag (next step continuously calls Drag)
                    draggedInteractable.DragStart();
                }
                else if (!dragStarted)
                {
                    // Consider starting Hold
                    if (!holdActive && heldTime >= holdStartTime && movedPixels <= holdMoveTolerancePixels)
                    {
                        holdActive = true;
                        pressInteractable.CursorHoldStart();
                    }
                }
            }
        }

        // Press end
        if (!pointerPressed && pointerWasPressed)
        {
            // Release
            isPointerDown = false;

            float totalHeldTime = Time.unscaledTime - pressTimeUnscaled;
            float movedPixels = Vector2.Distance(lastScreenPosition, pressScreenPosition);

            if (dragStarted && draggedInteractable != null)
            {
                endDrag();
            }
            else if (pressInteractable != null)
            {
                if (holdActive)
                {
                    pressInteractable.CursorHoldEnd();
                    holdActive = false;
                }
                else
                {
                    // Click if not dragged or held (also optionally require short duration and low movement)
                    if (totalHeldTime <= clickMaxTime && movedPixels <= holdMoveTolerancePixels)
                    {
                        pressInteractable.CursorClick();
                    }
                }
            }

            // Pair with the same interactable we pressed on
            if (pressInteractable != null)
            {
                pressInteractable.CursorSelectEnd();
            }

            // Reset
            pressInteractable = null;
            draggedInteractable = null;
            dragStarted = false;
            holdActive = false;
        }

        pointerWasPressed = pointerPressed;
    }

    private void processDrag()
    {
        if (draggedInteractable != null)
        {
            if (!dragStarted)
            {
                // Safety: in the new flow we call DragStart when starting drag;
                // this guard keeps legacy behavior if needed.
                draggedInteractable.DragStart();
                dragStarted = true;
            }
            else
            {
                draggedInteractable.Drag();
            }
        }
    }

    private void processHold()
    {
        if (isPointerDown && holdActive && pressInteractable != null && !dragStarted)
        {
            pressInteractable.CursorHold();
        }
    }

    private void endDrag()
    {
        if (draggedInteractable == null)
        {
            Debug.Log("Cursor endDrag while draggedInteractable is not set");
            return;
        }
        draggedInteractable.DragEnd();
        draggedInteractable = null;
        dragStarted = false;
    }

    private Vector2 updateWorldPosition()
    {
        Vector2 worldPostion = ScreenToWorld(lastScreenPosition);
        transform.position = worldPostion;
        return worldPostion;
    }

    private Vector2 ScreenToWorld(Vector2 screenPos)
    {
        var v3 = new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(v3);
    }

    private Interactable updateCurrentInteractable(Interactable current, Vector2 worldPosition, LayerMask layerMask)
    {
        var hitInteracrable = RaycastForInteractable(layerMask, worldPosition);

        if (hitInteracrable != current)
        {
            if (current != null)
            {
                current.CursorExit();
            }

            if (hitInteracrable != null)
            {
                hitInteracrable.CursorEnter();
            }
        }

        return hitInteracrable;
    }

    private Interactable RaycastForInteractable(LayerMask layerMask, Vector2 positoin)
    {
        Interactable hitInteractable = null;

        if (use3dPhysics)
        {
            var rayHits = Physics.SphereCastAll(positoin, 0.1f, Vector3.up, 0.1f, layerMask);
            if (rayHits.Length > 0)
                hitInteractable = rayHits[0].transform.GetComponentInParent<Interactable>();
        }
        else
        {
            var rayHits = Physics2D.RaycastAll(positoin, Vector2.zero, 1000f, layerMask);
            if (rayHits.Length > 0)
            {
                var tr = rayHits[0].rigidbody != null ? rayHits[0].rigidbody.transform : rayHits[0].transform;
                hitInteractable = tr.GetComponentInParent<Interactable>();
            }
            // add sorting and comparing if there are many
        }

        return hitInteractable;
    }
}
