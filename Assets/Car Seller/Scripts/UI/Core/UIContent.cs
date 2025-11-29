using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// implying Visitor pattern of the builder
/// </summary>
public interface IUIContent : IInteractionContent
{
    T BuildView<T>(IUIContentViewBuilder<T> builder);
    public event Action<IUIContent> onContentChanged;
    public string ProvideInfo { get; }
}


/// <summary>
/// add a normal constructor
/// </summary>
public abstract class UIContent : IUIContent
{
    public event Action<IUIContent> onContentChanged;

    public UIContent(Action[] modelEvents = null)
    {
        for(int i = 0; modelEvents != null && i < modelEvents.Length; i++)
            modelEvents[i] += () => onContentChanged?.Invoke(this);
    }

    abstract public T BuildView<T>(IUIContentViewBuilder<T> builder);
    public abstract string ProvideInfo{ get; }
}

public class UIContentList : UIContent
{
    public UIContentList(UIContent[] uIContents)
    {
        UIContents = uIContents;
    }
    public UIContent[] UIContents { get; private set; }

    public override T BuildView<T>(IUIContentViewBuilder<T> builder)
    {
        return builder.BuildList(this);
    }

    public override string ProvideInfo => $"UIContentList with {UIContents.Length} items.";
}