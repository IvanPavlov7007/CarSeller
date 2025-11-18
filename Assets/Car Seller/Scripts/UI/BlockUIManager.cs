using System.Collections;
using UnityEngine;
using Pixelplacement;
using System;

public class BlockUIManager : Singleton<BlockUIManager>
{
    CanvasGroup canvasGroup;
    Canvas canvas;

    public bool IsBlocking => focusCanvas != null;

    Canvas focusCanvas;
    int initialTargetSortingOrder;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Block(Canvas focusCanvas, Action onClick)
    {
        if (focusCanvas != null)
        {
            Debug.LogWarning($"BlockUIManager: Blocking UI with focus canvas {focusCanvas.name}");
            Unblock();
        }
        this.focusCanvas = focusCanvas;
        initialTargetSortingOrder = focusCanvas.sortingOrder;
        focusCanvas.sortingOrder = canvas.sortingOrder + 1;
        setBlock(true);
    }

    public void Unblock()
    {
        if (focusCanvas != null)
        {
            focusCanvas.sortingOrder = initialTargetSortingOrder;
            setBlock(false);
        }
        focusCanvas = null;
    }

    void setBlock(bool blocking)
    {
        canvasGroup.alpha = blocking ? 1 : 0;
        canvasGroup.blocksRaycasts = blocking;
        canvasGroup.interactable = blocking;
    }

}