using System.Collections;
using UnityEngine;
using Pixelplacement;
using System;

public class GameManager : Singleton<GameManager>
{
    public enum TimeState
    {
        Normal, Paused, FastForward
    }

    TimeState currentTimeState = TimeState.Normal;
    public TimeState CurrentTimeState { get { return currentTimeState; } }

    private void OnEnable()
    {
        UIInputController.Instance.onPaused += () => Pause(true);
        UIInputController.Instance.onResumed += () => Pause(false);
    }



    public void Pause(bool pause)
    {
        bool pausing = pause && currentTimeState != TimeState.Paused;
        bool unpausing = !pause && currentTimeState == TimeState.Paused;

        currentTimeState = pause ? TimeState.Paused : TimeState.Normal;
        changeTime();

        //if there was a change
        if (pausing || unpausing)
        {
            if(pause)
                PlayerInputLocator.Instance.PlayerInput.SwitchCurrentActionMap("UI");
            else
                PlayerInputLocator.Instance.PlayerInput.SwitchCurrentActionMap("Player");
        }

            // change notification
        if (pausing)
            GameEvents.Instance.OnGamePaused?.Invoke();
        else if (unpausing)
            GameEvents.Instance.OnGameUnpaused?.Invoke();
    }

    public void switchFastForward()
    {
        bool currentFastForward = currentTimeState == TimeState.FastForward;
        FastForward(!currentFastForward);
    }

    public void FastForward(bool fastForward)
    {
        if(currentTimeState != TimeState.Paused)
            currentTimeState = fastForward ? TimeState.FastForward : TimeState.Normal;
        changeTime();
    }

    public void ResetGame()
    {
        // delete the save data, reset the whole game from the beginning
    }

    void changeTime()
    {
        switch(currentTimeState)
        {
            case TimeState.Normal:
                Time.timeScale = 1f;
                break;
            case TimeState.Paused:
                Time.timeScale = 0f;
                break;
            case TimeState.FastForward:
                Time.timeScale = 2f;
                break;
        }
    }

}