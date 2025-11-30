using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using UnityEngine.UIElements;

public class PauseMenuManager : Singleton<PauseMenuManager>, IClosable
{
    CanvasGroup canvasGroup;
    Canvas canvas;
    [SerializeField]
    RectTransform container;

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

    public void Close()
    {
        GameManager.Instance.Pause(false);
    }

    void onPaused()
    {
        BlockUIManager.Instance.Block(canvas,()=> GameManager.Instance.Pause(false));
        SimpleUIBuilder.Instance?.Build(
            new UIElement
            {
                Type = UIElementType.Container,
                Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Text = "Pause Menu",
                        Type = UIElementType.Text
                    },
                    new UIElement
                    {
                        Text = "Resume",
                        Type = UIElementType.Button,
                        OnClick = () =>
                        {
                            GameManager.Instance.Pause(false);
                        }
                    },
                    new UIElement
                    {
                        Text = "Reset Game",
                        Type = UIElementType.Button,
                        OnClick = () =>
                        {
                            GameManager.Instance.ResetGame();
                        }
                    }

                }
            }

            , container);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    void onResumed()
    {
        BlockUIManager.Instance.Unblock(canvas);
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
    
}