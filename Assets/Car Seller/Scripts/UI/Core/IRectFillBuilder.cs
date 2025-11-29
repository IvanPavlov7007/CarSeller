using UnityEngine;

public interface IRectFillBuilder<T>
{
    T Build(RectTransform container, UIContentList contents);
}