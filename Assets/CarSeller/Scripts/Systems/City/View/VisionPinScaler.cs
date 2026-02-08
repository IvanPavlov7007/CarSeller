using UnityEngine;

/// <summary>
/// Applies the same vision-based scaling/hiding as `VisionDistanceScaler`, but to the UI pin.
/// Attach to `CityUIPin` GameObject.
/// </summary>
[DisallowMultipleComponent]
public sealed class VisionPinScaler : MonoBehaviour
{
    private CityUIPin _pin;
    private CityUIPinPositioner _positioner;
    private RectTransform _rect;
    private Vector3 _baseScale;

    private void Awake()
    {
        _pin = GetComponent<CityUIPin>();
        _positioner = GetComponent<CityUIPinPositioner>();
        _rect = GetComponent<RectTransform>();
        if (_rect != null) _baseScale = _rect.localScale;
    }

    private void LateUpdate()
    {
        if (_pin == null || _positioner == null || _positioner.target == null) return;

        var fog = VisualFogOfWarManager.Instance;
        if (fog == null)
        {
            ApplyVisible(true);
            ApplyScale(1f);
            return;
        }

        Vector2 worldPos = _positioner.target.position;

        if (!fog.TryGetNearestCenter(worldPos, out var center))
        {
            ApplyVisible(false);
            return;
        }

        float dist = Vector2.Distance(worldPos, center.Position);
        var cfg = center.Config;

        if (cfg.HideBeyondMax && dist > cfg.VisionMax)
        {
            ApplyVisible(false);
            return;
        }

        ApplyVisible(true);

        float s = cfg.EvaluateScale(dist);
        ApplyScale(s);

        // UI alpha via CanvasGroup if present.
        float a = cfg.EvaluateAlpha(dist);
        var cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = Mathf.Clamp01(a);
    }

    private void ApplyVisible(bool visible)
    {
        // Easiest for UI pins: activate/deactivate the whole GO.
        if (gameObject.activeSelf != visible)
            gameObject.SetActive(visible);
    }

    private void ApplyScale(float scaleMul)
    {
        if (_rect == null) return;
        _rect.localScale = _baseScale * Mathf.Max(0f, scaleMul);
    }
}
