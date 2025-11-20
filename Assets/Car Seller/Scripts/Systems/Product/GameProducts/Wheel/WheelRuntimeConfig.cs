using System.Collections;
using UnityEngine;

public sealed class WheelRuntimeConfig
{
    public WheelType wheelType;
    public GameObject frontSideViewPrefab;
    public GameObject backSideViewPrefab;
    public GameObject topViewPrefab;
    public Color color;
}

[System.Serializable]
public enum WheelType
{
    Sport,
    OffRoad,
    Super
}