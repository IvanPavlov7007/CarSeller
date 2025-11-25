using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "WheelBaseConfig", menuName = "Configs/Products/Wheel/Wheel Base Config")]
public class WheelBaseConfig : ScriptableObject, IBaseConfig
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