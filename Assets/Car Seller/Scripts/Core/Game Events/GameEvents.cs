using System.Collections;
using UnityEngine;
using Pixelplacement;
using System;

public class GameEvents : Singleton<GameEvents>
{
    public Action OnGamePaused;
    public Action OnGameUnpaused;

    public Action<ObjectWithPopupMenuTest> onObjectWithPopupClicked;
}