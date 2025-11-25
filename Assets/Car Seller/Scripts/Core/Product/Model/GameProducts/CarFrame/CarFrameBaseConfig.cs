using System.Collections;
using UnityEngine;
[CreateAssetMenu(fileName = "CarFrameBaseConfig", menuName = "Configs/Products/Frame/FrameBaseConfig")]
public class CarFrameBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name;
    public GameObject Prefab;
    public Color FrameColor;
    public Color WindshieldColor;
}