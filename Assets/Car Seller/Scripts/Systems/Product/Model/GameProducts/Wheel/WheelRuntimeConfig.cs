using System;
using System.Collections;
using UnityEngine;

[Serializable]
public sealed class WheelRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public WheelType WheelType;
    public Sprite FrontSideViewSprite;
    public Sprite BackSideViewSprite;
    public Sprite TopViewSprite;
    public Color Color;
    public float SideViewSize;
    public float TopViewSize;
}

[Serializable]
public enum WheelType
{
    Sport,
    OffRoad,
    Super
}