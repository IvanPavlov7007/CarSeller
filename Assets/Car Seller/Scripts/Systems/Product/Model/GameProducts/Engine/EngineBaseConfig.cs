using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "EngineBaseConfig", menuName = "Configs/Products/Engine/Engine Base Config")]
public class EngineBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name;
    public Sprite Sprite;
    public int Level;
}