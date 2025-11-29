public class HeaderContent : UIContent
{
    public HeaderContent(string header, string text, string providedInfo)
    {
        Header = header;
        Text = text;
        ProvidedInfo = providedInfo;
    }
    public string Header { get; private set; }
    public string Text { get; private set; }

    public string ProvidedInfo { get; private set; }
    public override string ProvideInfo => ProvidedInfo;

    public override T BuildView<T>(IUIContentViewBuilder<T> builder)
    {
        return builder.BuildHeader(this);
    }
}