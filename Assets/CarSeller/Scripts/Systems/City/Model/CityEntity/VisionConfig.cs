using System;
using UnityEngine;

/// <summary>
/// Defines how vision behaves around a center and how objects should be scaled/faded.
/// </summary>
[Serializable]
public sealed class VisionConfig
{
    [Min(0f)] public float Radius = 4f;

    [Header("Distance scaling")]
    [Min(0f)] public float VisionMin = 3f;
    [Min(0f)] public float VisionMax = 4f;

    [Header("Scale mapping")]
    [Range(0f, 10f)] public float ScaleAtMin = 1f;
    [Range(0f, 10f)] public float ScaleAtMax = 0.2f;

    public bool HideBeyondMax = true;

    public float EvaluateScale(float distance)
    {
        if (distance <= VisionMin) return ScaleAtMin;
        if (VisionMax <= VisionMin) return ScaleAtMax;
        float t = Mathf.InverseLerp(VisionMin, VisionMax, distance);
        return Mathf.Lerp(ScaleAtMin, ScaleAtMax, t);
    }

    public float EvaluateAlpha(float distance)
    {
        if (distance <= VisionMin) return 1f;
        if (VisionMax <= VisionMin) return 1f;
        float t = Mathf.InverseLerp(VisionMin, VisionMax, distance);
        return 1f - t;
    }
}
