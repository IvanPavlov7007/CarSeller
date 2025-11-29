public interface IUIContentViewBuilder<T>
{
    T BuildButton(ButtonContent buttonContent);
    T BuildHeader(HeaderContent headerContent);
    T BuildList(UIContentList contentList);
    T BuildText(TextContent textContent);
}