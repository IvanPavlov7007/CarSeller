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



    public void Pause(bool pause)
    {
        bool pausing = pause && currentTimeState != TimeState.Paused;
        bool unpausing = !pause && currentTimeState == TimeState.Paused;

        currentTimeState = pause ? TimeState.Paused : TimeState.Normal;
        // change notification
        if (pausing)
            GameEvents.Instance.OnGamePaused?.Invoke();
        else if (unpausing)
            GameEvents.Instance.OnGameUnpaused?.Invoke();

        changeTime();
    }

    public void FastForward(bool fastForward)
    {
        if(currentTimeState != TimeState.Paused)
            currentTimeState = fastForward ? TimeState.FastForward : TimeState.Normal;
        changeTime();
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