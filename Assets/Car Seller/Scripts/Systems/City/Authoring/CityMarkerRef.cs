using System;
using UnityEngine;

[Serializable]
public struct CityMarkerRef
{
    public CityGraphAsset Graph;
    public string MarkerId;

    public bool IsValid => Graph != null && !string.IsNullOrEmpty(MarkerId);
}