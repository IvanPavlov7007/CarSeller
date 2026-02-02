using Pixelplacement;
using UnityEngine;
using UnityEngine.UI;

public class CityUIManager : Singleton<CityUIManager>
{
    private Camera _cam;
    private Canvas _canvas;
    public Camera Camera
    {
        get
        {
            if(_cam == null)
                _cam = Camera.main;
            return _cam;
        }
    }
    public Canvas Canvas
    {
        get
        {
            if (_canvas == null)
                _canvas = gameObject.GetComponent<Canvas>();
            return _canvas;
        }
    }
}

public class CityUIBuilder
{
    public CityUIPin SetUpCityPin(CityViewObjectController cityViewObjectController, GameObject pinPrefab, PinStyle pinStyle)
    {
        Debug.Assert(cityViewObjectController != null);
        Debug.Assert(pinPrefab != null);
        Debug.Assert(pinStyle != null);
        Debug.Assert(CityUIManager.Instance != null);

        CityUIPin pinInstance = GameObject.Instantiate(pinPrefab, CityUIManager.Instance.Canvas.transform).AddComponent<CityUIPin>();
        pinInstance.Initialize(cityViewObjectController, pinStyle);
        return pinInstance;
    }
}