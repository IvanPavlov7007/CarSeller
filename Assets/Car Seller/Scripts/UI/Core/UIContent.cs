using System.Collections;
using UnityEngine;
using System;

public struct UIContent
{
    public string Header;
    public string Text;
    public Sprite Image;
    public UIContentType ContentType;
    public Action pushAction;
    public Action<bool> interactive;
    public Action<string> info;
    public event Action OnContentChanged;
}

public enum  UIContentType
{
    Header, Text, Button
}