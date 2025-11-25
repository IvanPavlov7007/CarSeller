using UnityEngine;

[CreateAssetMenu(fileName = "SpoilerBaseConfig", menuName = "Configs/Products/Spoiler/Spoiler Base Config")]
public class SpoilerBaseConfig : ScriptableObject, IBaseConfig
{
    public string Name;
    public Sprite Sprite;
    public Color Color;
    public float Size;
}