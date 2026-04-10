using Pixelplacement;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputLocator : Singleton<PlayerInputLocator>
{
    [SerializeField]
    PlayerInput playerInput;
    public PlayerInput PlayerInput => playerInput;
}