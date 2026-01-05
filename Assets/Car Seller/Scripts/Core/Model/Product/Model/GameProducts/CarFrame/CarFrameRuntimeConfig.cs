using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class CarFrameRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public GameObject Prefab;
    public Color FrameColor;
    public Color WindshieldColor;
    public Sprite Icon;
    public float BasePrice;
}