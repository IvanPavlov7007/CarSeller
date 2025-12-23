using System;
using System.Collections.Generic;
using UnityEngine;

public class UIElement : IInteractionContent
{
    public UIElementType Type;
    public List<UIElement> Children;
    public string Text;
    public string info;
    public Sprite Image;
    public bool IsInteractable = true;
    public bool closePopupOnClick = true;
    public bool blockingInput = false;
    public Action OnClick;
    public string UnavailabilityReason;
    public object CustomData; // widget-specific data
    public string Style; // optional layout classes
}

public enum UIElementType
{
    Container,
    Text,
    Image,
    Button,
    Spacer,
    CustomWidget, // for things like "wheel panel"
}