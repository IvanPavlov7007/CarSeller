using System;
using UnityEngine;

/// <summary>
/// Marks this entity as a vision center.
/// The fog will reveal an area around it with the given radius.
/// Also provides the distance scaling configuration applied to other entities.
/// </summary>
public sealed class VisionCenterAspect : CityEntityAspect
{
    public readonly VisionConfig Config;

    public VisionCenterAspect(VisionConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public VisionCenterAspect(float radius)
    {
        Config = new VisionConfig { Radius = Mathf.Max(0f, radius) };
    }
}

/// <summary>
/// Enables scaling / fading with distance to the nearest vision center.
/// The scaling curve/ranges are taken from the nearest vision center's <see cref="VisionCenterAspect"/>.
/// </summary>
public sealed class VisionDistanceScaleAspect : CityEntityAspect
{
    /// <summary>
    /// When true, this entity is always rendered at its base scale and never hidden.
    /// </summary>
    public readonly bool Disable;

    public VisionDistanceScaleAspect(bool disable = false)
    {
        Disable = disable;
    }
}
