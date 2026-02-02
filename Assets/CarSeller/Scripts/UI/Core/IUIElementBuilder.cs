using UnityEngine;

public interface IUIElementBuilder<T>
{
    T Build(UIElement content, RectTransform container);
}