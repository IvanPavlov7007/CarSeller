using UnityEngine;
using UnityEngine.EventSystems;

public class CityUIPinPositioner : MonoBehaviour
{
    public Camera cam { get; private set; }
    public Canvas canvas { get; private set; }
    public Transform origin { get; private set; }
    public Transform target { get; private set; }

    public RectTransform IconRectTransform => iconTransform;
    public RectTransform FrameRectTransform => frameTransform;

    public float screenEdgeMargin = 50f;
    public float targetOnScreenMargin = 50f;

    CityUIPinDragHandler dragHandler;
    Quaternion userRotation = Quaternion.identity;

    [SerializeField]
    RectTransform iconTransform;
    [SerializeField]
    RectTransform frameTransform;
    RectTransform rectTransform;
    Quaternion initialIconRotation;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialIconRotation = iconTransform.rotation;
        dragHandler = GetComponentInChildren<CityUIPinDragHandler>();
        dragHandler.OnCustomDrag += OnDrag;
        dragHandler.OnCustomBeginDrag += OnBeginDrag;
    }

    public void Initialize(Camera cam, Canvas canvas, Transform origin, Transform target)
    {
        this.cam = cam;
        this.canvas = canvas;
        this.origin = origin;
        this.target = target;
    }

    private void LateUpdate()
    {
        RelocatePin();
        iconTransform.rotation = initialIconRotation;
    }

    void RelocatePin()
    {
        if (cam == null)
        {
            Debug.LogWarning("No camera registered");
            return;
        }

        if (canvas == null)
        {
            Debug.LogWarning("No canvas registered");
            return;
        }

        Vector2 distanceToTarget = target.position - origin.position;
        Vector2 directionToTarget = distanceToTarget.normalized;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 screenPos;

        // Is target within camera world rect?
        if (cameraWorldSizeRect(cam).Contains(target.position))
        {
            // Target on screen: start from its screen position
            screenPos = cam.WorldToScreenPoint(target.position);
            screenPos -= directionToTarget * targetOnScreenMargin;
        }
        else
        {
            // Target off screen: clamp to screen edge
            Vector2 edgePoint = GetEdgePoint(directionToTarget, screenSize);
            screenPos = edgePoint - directionToTarget * screenEdgeMargin;
            userRotation = Quaternion.identity; // reset user rotation when target is offscreen
        }

        // Convert screen position to canvas local position.
        // For Screen Space - Camera, we must pass the canvas' render camera.
        Camera canvasCamera = canvas.worldCamera != null ? canvas.worldCamera : cam;

        Vector2 localPos;
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvasCamera,
            out localPos);

        rectTransform.anchoredPosition = localPos;
        rectTransform.up = userRotation * - directionToTarget;
    }

    public static Vector2 worldScreenHalfSize(Camera cam)
    {
        if (cam.orthographic)
        {
            float height = cam.orthographicSize;
            float width = height * cam.aspect;

            return new Vector2(width, height);
        }
        else
        {
            // For perspective, use field of view and distance
            float height = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * -cam.transform.position.z;
            float width = height * cam.aspect;
            return new Vector2(width, height);
        }
    }

    public static Rect cameraWorldSizeRect(Camera cam)
    {
        var halfSize = worldScreenHalfSize(cam);
        Rect rect = new Rect(Vector2.zero, halfSize * 2f);
        rect.center = cam.transform.position;
        return rect;
    }

    static Vector2 GetEdgePoint(Vector2 direction, Vector2 screenSize)
    {
        // operate in screen coordinates where (0,0) is bottom-left and (w,h) is top-right
        Vector2 screenCenter = screenSize * 0.5f;

        float tMax = float.MaxValue;

        // Left or Right edge
        if (!Mathf.Approximately(direction.x, 0f))
        {
            float tx1 = (0f - screenCenter.x) / direction.x;           // left edge (x = 0)
            float tx2 = (screenSize.x - screenCenter.x) / direction.x; // right edge (x = width)
            if (tx1 > 0f) tMax = Mathf.Min(tMax, tx1);
            if (tx2 > 0f) tMax = Mathf.Min(tMax, tx2);
        }

        // Top or Bottom edge
        if (!Mathf.Approximately(direction.y, 0f))
        {
            float ty1 = (0f - screenCenter.y) / direction.y;           // bottom edge (y = 0)
            float ty2 = (screenSize.y - screenCenter.y) / direction.y; // top edge (y = height)
            if (ty1 > 0f) tMax = Mathf.Min(tMax, ty1);
            if (ty2 > 0f) tMax = Mathf.Min(tMax, ty2);
        }

        Vector2 edgeOffsetFromCenter = direction * tMax;
        return screenCenter + edgeOffsetFromCenter;
    }

    Vector2 previousDir;

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousDir = GetDirectionFromCenter(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentDir = GetDirectionFromCenter(eventData.position);

        // angle between previous and current drag direction (signed around Z)
        float deltaAngle = Vector2.SignedAngle(previousDir, currentDir);

        userRotation *= Quaternion.Euler(0f, 0f, deltaAngle);

        previousDir = currentDir;
    }

    Vector2 GetDirectionFromCenter(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            frameTransform,
            screenPos,
            cam,
            out Vector3 worldPoint);
        return (worldPoint - rectTransform.position).normalized;
    }

}