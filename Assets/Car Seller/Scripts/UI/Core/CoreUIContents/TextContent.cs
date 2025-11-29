using UnityEngine;

public class TextContent : UIContent
{
    public TextContent(string header, string text)
    {
        Header = header;
        Text = text;
    }

    public Sprite Image { get; private set; }
    public string Header { get; private set; }
    public string Text { get; private set; }
    public override T BuildView<T>(IUIContentViewBuilder<T> builder)
    {
        return builder.BuildText(this);
    }
}