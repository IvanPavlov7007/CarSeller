using UnityEngine;
using UnityEngine.InputSystem;
using Pixelplacement;
using System;

// Deprecated: input now handled directly in GameCursor via UnityEngine.InputSystem.
[Obsolete("DirectInput is no longer used. GameCursor polls Mouse/Touch directly.")]
public class DirectInput : Singleton<DirectInput>
{
    public event Action<bool> onClick;
    public event Action<Vector2> onPoint;

    public Vector2 lastPosition;
}
