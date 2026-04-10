using UnityEngine;
using Pixelplacement;

public class PauseMenuManager : Singleton<PauseMenuManager>, IClosable
{
    CanvasGroup canvasGroup;
    Canvas canvas;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnGamePaused += onPaused;
        GameEvents.Instance.OnGameUnpaused += onResumed;
        onResumed();
    }

    public void Close()
    {
        GameManager.Instance.Pause(false);
    }

    void onPaused()
    {
        BlockUIManager.Instance.Block(canvas,()=> GameManager.Instance.Pause(false));
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    void onResumed()
    {
        BlockUIManager.Instance.Unblock(canvas);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
    
}