using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class EngineRuntimeConfig : IRuntimeConfig
{
    public string Name;
    public Sprite Sprite;
    public int Level;
}