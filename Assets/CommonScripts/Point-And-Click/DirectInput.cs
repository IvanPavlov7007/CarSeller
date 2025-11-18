using UnityEngine;
using UnityEngine.InputSystem;
using Pixelplacement;
using System;

public class DirectInput : Singleton<DirectInput>
{
    public event Action<bool> onClick;
    public event Action<Vector2> onPoint;

    public Vector2 lastPosition;

    public void OnClick(InputValue value)
    {
        //Debug.Log(value.ToString());
        onClick?.Invoke(value.isPressed);
    }

    public void OnPoint(InputValue value)
    {
        //Debug.Log(value.Get<Vector2>());
        lastPosition = value.Get<Vector2>();
        onPoint?.Invoke(lastPosition);
    }

    public void test()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
    }
}
