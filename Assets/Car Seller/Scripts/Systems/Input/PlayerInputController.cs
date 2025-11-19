using Pixelplacement;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputController : Singleton<PlayerInputController>
{
    public void OnSprint(InputValue inputValue)
    {
        GameManager.Instance.switchFastForward();
    }
}