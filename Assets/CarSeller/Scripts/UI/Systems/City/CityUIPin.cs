using System;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;

public class CityUIPin : MonoBehaviour
{
    CityUIPinPositioner positioner;
    CityViewObjectController cityViewObjectController;

    Interactable interactable => cityViewObjectController.GetComponent<Interactable>();

    private void Awake()
    {
        positioner = GetComponent<CityUIPinPositioner>();
    }

    public void Initialize(CityViewObjectController cityViewObjectController, PinStyle pinStyle)
    {
        this.cityViewObjectController = cityViewObjectController;
        Camera cam = CityUIManager.Instance.Camera;
        Canvas canvas = CityUIManager.Instance.Canvas;
        positioner.Initialize(cam, canvas, cam.transform, cityViewObjectController.transform);

        var txt = positioner.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = pinStyle.Text;
        txt.color = pinStyle.ForegroundColor;
        positioner.FrameRectTransform.GetComponent<Image>().color = pinStyle.BackgroundColor;
        var iconImage = positioner.IconRectTransform.GetComponent<Image>();
        iconImage.sprite = pinStyle.Icon;
        if(pinStyle.Icon != null)
            iconImage.color = pinStyle.ForegroundColor;
        else
            iconImage.color = Color.clear;

        gameObject.AddComponent<ViewStateChangerUI>().Initialize(cityViewObjectController);

        // New: mirror vision scaling onto the UI pin.
        if (gameObject.GetComponent<VisionPinScaler>() == null)
            gameObject.AddComponent<VisionPinScaler>();

        cityViewObjectController.OnDestroyed += handleObjectDestroyed;
        gameObject.GetComponentInChildren<Button>().onClick.AddListener(onClick);
    }

    private void OnDisable()
    {
        if (cityViewObjectController != null)
        cityViewObjectController.OnDestroyed -= handleObjectDestroyed;

        var btn = gameObject.GetComponentInChildren<Button>();
        if (btn != null)
            btn.onClick.RemoveListener(onClick);
    }

    private void handleObjectDestroyed()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }

    private void onClick()
    {
        Debug.Assert(interactable != null, "CityUIPin: No Interactable component found on the CityViewObjectController.");
        if(positioner.IsDragging)
            return;
        interactable?.CursorClick();
    }

}