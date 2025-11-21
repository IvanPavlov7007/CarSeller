using System;
using System.Collections;
using UnityEngine;

[Serializable]
public sealed class WheelRuntimeConfig
{
    public WheelType wheelType;
    public GameObject frontSideViewPrefab;
    public GameObject backSideViewPrefab;
    public GameObject topViewPrefab;
    public Color color;
}

[Serializable]
public enum WheelType
{
    Sport,
    OffRoad,
    Super
}