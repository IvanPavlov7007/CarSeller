using System.Linq;
using UnityEngine;

/// <summary>
/// Scales and optionally hides a city entity view based on distance to the nearest active vision center.
/// Requires `VisionDistanceScaleAspect` on the underlying `CityEntity`.
/// </summary>
[DisallowMultipleComponent]
public sealed class VisionDistanceScaler : MonoBehaviour
{
    private CityViewObjectController _controller;
    private Transform _root;

    private VisionDistanceScaleAspect _aspect;

    private Vector3 _baseLocalScale;

    private Renderer[] _renderers;
    private CanvasGroup[] _canvasGroups;

    private bool _hasInit;

    private void Awake()
    {
        _controller = GetComponent<CityViewObjectController>();
        _root = transform;
        _baseLocalScale = _root.localScale;

        _renderers = GetComponentsInChildren<Renderer>(true);
        _canvasGroups = GetComponentsInChildren<CanvasGroup>(true);
    }

    private void OnEnable()
    {
        TryResolveAspect();
        _hasInit = true;
    }

    private void LateUpdate()
    {
        if (!_hasInit) return;

        if (_controller == null || _controller.CityEntity == null)
            return;

        if (!TryResolveAspect() || _aspect.Disable)
            return;

        var fog = CityFogRenderer.Instance;
        if (fog == null)
        {
            ApplyVisibility(visible: true);
            return;
        }

        Vector2 myPos = _controller.CityEntity.Position.WorldPosition;

        if (!G.CityVision.TryGetNearestCenter(myPos, out var centerEntity, out var centerAspect) || centerAspect?.Config == null)
        {
            ApplyVisibility(visible: false);
            return;
        }

        float dist = Vector2.Distance(myPos, centerEntity.Position.WorldPosition);
        var cfg = centerAspect.Config;

        if (cfg.HideBeyondMax && dist > cfg.VisionMax)
        {
            ApplyVisibility(visible: false);
            return;
        }

        ApplyVisibility(visible: true);

        float scaleMul = cfg.EvaluateScale(dist);
        _root.localScale = _baseLocalScale * Mathf.Max(0f, scaleMul);

        ApplyAlpha(cfg.EvaluateAlpha(dist));
    }

    private bool TryResolveAspect()
    {
        if (_controller == null || _controller.CityEntity == null) return false;

        var a = _controller.CityEntity.Aspects?.OfType<VisionDistanceScaleAspect>().FirstOrDefault();
        if (a == null) return false;

        _aspect = a;
        return true;
    }

    private void ApplyVisibility(bool visible)
    {
        if (_renderers != null && _renderers.Length > 0)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null) _renderers[i].enabled = visible;
            }
        }

        if (_canvasGroups != null && _canvasGroups.Length > 0)
        {
            for (int i = 0; i < _canvasGroups.Length; i++)
            {
                var cg = _canvasGroups[i];
                if (cg == null) continue;
                if (!visible) cg.alpha = 0f;
                cg.interactable = visible;
                cg.blocksRaycasts = visible;
            }
        }

        var cols2d = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < cols2d.Length; i++)
        {
            if (cols2d[i] != null) cols2d[i].enabled = visible;
        }

        var cols3d = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols3d.Length; i++)
        {
            if (cols3d[i] != null) cols3d[i].enabled = visible;
        }
    }

    private void ApplyAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        if (_canvasGroups != null && _canvasGroups.Length > 0)
        {
            for (int i = 0; i < _canvasGroups.Length; i++)
            {
                var cg = _canvasGroups[i];
                if (cg != null) cg.alpha = alpha;
            }
            return;
        }

        var srs = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
        {
            var sr = srs[i];
            if (sr == null) continue;
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        var gs = GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        for (int i = 0; i < gs.Length; i++)
        {
            var g = gs[i];
            if (g == null) continue;
            var c = g.color;
            c.a = alpha;
            g.color = c;
        }
    }
}
