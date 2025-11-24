using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "CarFrameBaseConfig", menuName = "Configs/Products/Frame/Frame Base Config")]
public class CarFrameBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name;
    public Sprite Sprite;
    public Color FrameColor;
    public Color WindshieldColor;
}