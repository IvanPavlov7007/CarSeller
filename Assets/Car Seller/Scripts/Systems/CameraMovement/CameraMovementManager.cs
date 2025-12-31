using Pixelplacement;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovementManager : Singleton<CameraMovementManager>
{
    [Header("References")]
    public Transform cameraTarget;
    public GameObject camConfiner; // GameObject holding the 2D collider used by CinemachineConfiner2D

    [Header("Drag Settings")]
    [Tooltip("Enable/disable camera dragging.")]
    public bool enableDrag = true;
    [Tooltip("Multiplier to tune drag responsiveness.")]
    [Range(0.05f, 5f)]
    public float dragSensitivity = 1.0f;
    [Tooltip("Curve remapping pointer delta magnitude (in world units) to applied delta. Optional softening at small deltas.")]
    public AnimationCurve sensitivityCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [Tooltip("When true, dragging is disabled while the pointer is over UI.")]
    public bool blockDragOverUI = true;

    [Header("Feel")]
    [Tooltip("Apply simple inertia/smoothing to camera target movement.")]
    public bool useInertia = true;
    [Range(0f, 1f)]
    [Tooltip("0 = instant, 1 = very slow. Lerp factor toward target position per frame when inertia is enabled.")]
    public float inertiaLerp = 0.2f;

    [Header("Follow Dragged MovingPoint")]
    [Tooltip("When true, camera target will automatically track a dragged MovingPoint.")]
    public bool followDraggedMovingPoint = true;
    [Tooltip("How strongly the camera centers on the dragged MovingPoint (0 = no follow, 1 = hard lock).")]
    [Range(0f, 1f)]
    public float followStrength = 0.25f;

    Camera cam => Camera.main;

    // Drag state
    bool _isDragging;
    Vector2 _lastPointerScreenPos;
    Vector3 _pendingTargetPos; // desired target position when using inertia

    void Update()
    {
        if (cam == null || cameraTarget == null) return;

        var cursor = GameCursor.Instance;
        Interactable dragged = cursor != null ? cursor.draggedInteractable : null;

        // If we are dragging a MovingPoint, bias the camera toward it.
        if (followDraggedMovingPoint && dragged != null)
        {
            var movingPoint = dragged.GetComponent<MovingPoint>();
            if (movingPoint != null)
            {
                FollowMovingPoint(movingPoint);
            }
        }

        // Dragging the camera with pointer is disabled while GameCursor is interacting.
        if (!enableDrag) return;

        HandlePointerPan(cursor);
    }

    void HandlePointerPan(GameCursor cursor)
    {
        // Choose input source: prefer touch ONLY when an actual touch is active; otherwise mouse.
        bool pointerPressed;
        bool pressedThisFrame;
        bool releasedThisFrame;
        Vector2 pointerPos;

        if (cam == null) return;

        var touch = Touchscreen.current?.primaryTouch;
        bool touchActive = touch != null && (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame);

        if (touchActive)
        {
            pointerPos = touch.position.ReadValue();
            pointerPressed = touch.press.isPressed;
            pressedThisFrame = touch.press.wasPressedThisFrame;
            releasedThisFrame = touch.press.wasReleasedThisFrame ||
                                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
        }
        else if (Mouse.current != null)
        {
            pointerPos = Mouse.current.position.ReadValue();
            pointerPressed = Mouse.current.leftButton.isPressed;
            pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;
        }
        else
        {
            return; // no pointer available
        }

        // Block when GameCursor is interacting (press/drag/hold or UI handling)
        bool cursorBlocking = cursor != null && cursor.IsInteracting;

        // Optionally block when pointer is over UI (when we don’t have cursor or want extra protection)
        bool overUI = false;
        if (blockDragOverUI)
        {
            overUI = IsPointerOverUI(pointerPos);
        }

        // Begin drag only if:
        // - pointer pressed this frame
        // - not interacting via GameCursor
        // - not over UI (if enabled)
        if (pressedThisFrame && !cursorBlocking && !overUI)
        {
            _isDragging = true;
            _lastPointerScreenPos = pointerPos;
            _pendingTargetPos = cameraTarget.position;
        }

        // End drag
        if (releasedThisFrame)
        {
            _isDragging = false;
        }

        // Apply drag movement
        if (_isDragging && pointerPressed)
        {
            // Compute world delta from screen movement on the camera-target plane
            Vector3 prev = ScreenToWorldOnTargetPlane(_lastPointerScreenPos);
            Vector3 curr = ScreenToWorldOnTargetPlane(pointerPos);
            Vector3 worldDelta = curr - prev;

            // Optional sensitivity curve (remap magnitude)
            float mag = worldDelta.magnitude;
            if (mag > 0.0001f)
            {
                float scale = sensitivityCurve != null ? sensitivityCurve.Evaluate(Mathf.Clamp01(mag)) : 1f;
                worldDelta = worldDelta.normalized * (mag * scale);
            }

            // Move follow target opposite to pointer delta (natural pan)
            Vector3 desired = _pendingTargetPos - worldDelta * dragSensitivity;

            // Clamp desired to confiner with camera half-extents considered
            desired = ClampToConfinerShrunk(desired);

            // Commit desired (either instantly or via inertia)
            if (useInertia)
            {
                _pendingTargetPos = desired;
                cameraTarget.position = Vector3.Lerp(cameraTarget.position, _pendingTargetPos, 1f - Mathf.Pow(1f - inertiaLerp, Time.unscaledDeltaTime * 60f));
            }
            else
            {
                _pendingTargetPos = desired;
                cameraTarget.position = desired;
            }

            _lastPointerScreenPos = pointerPos;
        }
        else if (useInertia)
        {
            // Continue easing toward pending target when not actively dragging
            cameraTarget.position = Vector3.Lerp(cameraTarget.position, _pendingTargetPos, 1f - Mathf.Pow(1f - inertiaLerp, Time.unscaledDeltaTime * 60f));
        }
    }

    void FollowMovingPoint(MovingPoint movingPoint)
    {
        // Target the MovingPoint's world position.
        Vector3 mpPos = movingPoint.transform.position;

        // Blend from current (or pending) toward moving point by followStrength.
        Vector3 desired = Vector3.Lerp(cameraTarget.position, mpPos, followStrength);

        // Respect confiner.
        desired = ClampToConfinerShrunk(desired);

        _pendingTargetPos = desired;

        if (useInertia)
        {
            cameraTarget.position = Vector3.Lerp(
                cameraTarget.position,
                _pendingTargetPos,
                1f - Mathf.Pow(1f - inertiaLerp, Time.unscaledDeltaTime * 60f));
        }
        else
        {
            cameraTarget.position = desired;
        }
    }

    // Accurate world conversion for orthographic: project onto the plane of the camera target (its z)
    Vector3 ScreenToWorldOnTargetPlane(Vector2 screenPos)
    {
        float zDistanceFromCamera = cameraTarget.position.z - cam.transform.position.z;
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistanceFromCamera));
    }

    // Shrink confiner bounds by camera half-extents so the camera can actually move even if the target starts near edges.
    Vector3 ClampToConfinerShrunk(Vector3 desired)
    {
        if (camConfiner == null) return desired;

        var col2d = camConfiner.GetComponent<Collider2D>();
        if (col2d == null)
        {
            // Fallback to 3D bounds if present
            var col3d = camConfiner.GetComponent<Collider>();
            if (col3d == null) return desired;

            Bounds b3 = col3d.bounds;
            Vector2 halfExtents = GetCameraHalfExtentsWorld();
            float minX = b3.min.x + halfExtents.x;
            float maxX = b3.max.x - halfExtents.x;
            float minY = b3.min.y + halfExtents.y;
            float maxY = b3.max.y - halfExtents.y;
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
            return desired;
        }

        Bounds b = col2d.bounds;
        Vector2 half = GetCameraHalfExtentsWorld();

        // If bounds are smaller than camera view, keep target at center of bounds
        float minX2 = b.min.x + half.x;
        float maxX2 = b.max.x - half.x;
        float minY2 = b.min.y + half.y;
        float maxY2 = b.max.y - half.y;

        if (minX2 > maxX2) desired.x = b.center.x;
        else desired.x = Mathf.Clamp(desired.x, minX2, maxX2);

        if (minY2 > maxY2) desired.y = b.center.y;
        else desired.y = Mathf.Clamp(desired.y, minY2, maxY2);

        return desired;
    }

    // Camera half extents in world units for orthographic camera
    Vector2 GetCameraHalfExtentsWorld()
    {
        if (!cam.orthographic)
        {
            // Approximate by projecting screen corners to target plane
            Vector3 c = ScreenToWorldOnTargetPlane(new Vector2(cam.pixelWidth * 0.5f, cam.pixelHeight * 0.5f));
            Vector3 r = ScreenToWorldOnTargetPlane(new Vector2(cam.pixelWidth, cam.pixelHeight * 0.5f));
            Vector3 t = ScreenToWorldOnTargetPlane(new Vector2(cam.pixelWidth * 0.5f, cam.pixelHeight));
            return new Vector2(Mathf.Abs(r.x - c.x), Mathf.Abs(t.y - c.y));
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        return new Vector2(halfWidth, halfHeight);
    }

    // Basic UI check without depending on GameCursor internals
    bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = screenPosition
        };
        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);
        bool over = results.Count > 0;
        results.Clear();
        return over;
    }
}