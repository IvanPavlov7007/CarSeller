using System.Collections;
using UnityEngine;
using System;

public class UIContent : IInteractionContent
{
    public UIContent(UISingleContent[] uIContents)
    {
        UIContents = uIContents;
    }

    public static UIContent Error => 
        new UIContent(new UISingleContent[1] { UISingleContent.Error});
    public UISingleContent[] UIContents { get; private set; }
}

/// <summary>
/// add a normal constructor
/// </summary>
public class UISingleContent
{
    public static UISingleContent Error => 
        new UISingleContent { Header = "Error", Text = "Error", ContentType = UIContentType.Header};

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