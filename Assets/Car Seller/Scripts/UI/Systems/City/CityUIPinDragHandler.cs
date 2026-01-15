using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CityUIPinDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public event Action<PointerEventData> OnCustomDrag;
    public event Action<PointerEventData> OnCustomBeginDrag;
    public void OnBeginDrag(PointerEventData eventData)
    {
        OnCustomBeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnCustomDrag?.Invoke(eventData);
    }
}