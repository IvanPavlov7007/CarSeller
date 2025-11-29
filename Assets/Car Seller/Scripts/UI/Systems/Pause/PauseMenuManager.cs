using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class PauseMenuManager : Singleton<PauseMenuManager>
{
    CanvasGroup canvasGroup;
    Canvas canvas;
    [SerializeField]
    RectTransform container;

    PauseContentProvider contentProvider = new PauseContentProvider();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        GameEvents.Instance.OnGamePaused += onPaused;
        GameEvents.Instance.OnGameUnpaused += onResumed;
    }

    void onPaused()
    {
        BlockUIManager.Instance.Block(canvas,()=> GameManager.Instance.Pause(false));
        SimpleUIBuilder.Instance?.Build(container, contentProvider.GetContents(new UIContext()));
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    void onResumed()
    {
        BlockUIManager.Instance.Unblock(canvas);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }


    private class PauseContentProvider : UIContentProvider
    {
        public IEnumerable<UIContent> GetContents(UIContext context)
        {
            List<UIContent> contents = new List<UIContent>
            {
                new UIContent { Header = "Pause Menu", ContentType = UIContentType.Header },
                new UIContent
                {
                    Text = "Resume",
                    ContentType = UIContentType.Button,
                    pushAction = () =>
                    {
                        GameManager.Instance.Pause(false);
                    }
                },
                new UIContent
                {
                    Text = "Reset Game",
                    ContentType = UIContentType.Button,
                    pushAction = () =>
                    {
                        GameManager.Instance.ResetGame();
                    }
                }
            };

            return contents;
        }
    }
    
}