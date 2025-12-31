using System;
using UnityEngine;
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

    public void Initialize(CityViewObjectController cityViewObjectController, Sprite icon)
    {
        this.cityViewObjectController = cityViewObjectController;
        Camera cam = CityUIManager.Instance.Camera;
        Canvas canvas = CityUIManager.Instance.Canvas;
        positioner.Initialize(cam, canvas, cam.transform, cityViewObjectController.transform);
        gameObject.AddComponent<ViewStateChangerUI>().Initialize(cityViewObjectController);
        positioner.IconRectTransform.GetComponent<Image>().sprite = icon;

        cityViewObjectController.OnDestroyed += handleObjectDestroyed;
        gameObject.GetComponentInChildren<Button>().onClick.AddListener(onClick);
    }

    private void OnDisable()
    {
        cityViewObjectController.OnDestroyed -= handleObjectDestroyed;
        gameObject.GetComponentInChildren<Button>().onClick.AddListener(onClick);
    }

    private void handleObjectDestroyed()
    {
        if(gameObject != null)
            Destroy(gameObject);
    }

    private void onClick()
    {
        Debug.Assert(interactable != null, "CityUIPin: No Interactable component found on the CityViewObjectController.");
        interactable?.CursorClick();
    }

}