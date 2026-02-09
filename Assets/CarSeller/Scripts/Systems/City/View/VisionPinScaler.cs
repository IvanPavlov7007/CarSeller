using UnityEngine;

/// <summary>
/// Applies the same vision-based scaling/hiding as `VisionDistanceScaler`, but to the UI pin.
/// Attach to `CityUIPin` GameObject.
/// </summary>
[DisallowMultipleComponent]
public sealed class VisionPinScaler : MonoBehaviour
{
    private CityUIPinPositioner _positioner;
    private RectTransform _rect;
    private Vector3 _baseScale;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _positioner = GetComponent<CityUIPinPositioner>();
        _rect = GetComponent<RectTransform>();
        if (_rect != null) _baseScale = _rect.localScale;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void LateUpdate()
    {
        if (_positioner == null || _positioner.target == null) return;

        Vector2 worldPos = _positioner.target.position;

        if (!G.CityVision.TryGetNearestCenter(worldPos, out var centerEntity, out var centerAspect) || centerAspect?.Config == null)
        {
            ApplyVisible(false);
            return;
        }

        float dist = Vector2.Distance(worldPos, centerEntity.Position.WorldPosition);
        var cfg = centerAspect.Config;

        if (cfg.HideBeyondMax && dist > cfg.VisionMax)
        {
            ApplyVisible(false);
            return;
        }

        ApplyVisible(true);

        float s = cfg.EvaluateScale(dist);
        ApplyScale(s);

        // UI alpha fade (also used for smoothing within min/max).
        float a = cfg.EvaluateAlpha(dist);
        _canvasGroup.alpha = Mathf.Clamp01(a);
    }

    private void ApplyVisible(bool visible)
    {
        // Do NOT SetActive(false) here, otherwise LateUpdate stops and the pin can never become visible again.
        _canvasGroup.alpha = visible ? _canvasGroup.alpha : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    private void ApplyScale(float scaleMul)
    {
        if (_rect == null) return;
        _rect.localScale = _baseScale * Mathf.Max(0f, scaleMul);
    }
}
