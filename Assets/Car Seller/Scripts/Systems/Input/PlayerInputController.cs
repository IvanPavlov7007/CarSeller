using Pixelplacement;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputController : Singleton<PlayerInputController>
{
    public event Action crouched;
    public event Action jumped;

    public void OnSprint(InputValue inputValue)
    {
        GameManager.Instance.switchFastForward();
    }

    //Debugging purpose
    public void OnCrouch(InputValue inputValue)
    {
        crouched?.Invoke();
    }

    public void OnJump(InputValue inputValue)
    {
        jumped?.Invoke();
    }
}