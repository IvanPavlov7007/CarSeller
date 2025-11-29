using System;

public class ButtonContent : UIContent
{
    public ButtonContent(string header, string text, Action pushAction, UnavailabilityReason unavailabilityReason = null)
    {
        Header = header;
        Text = text;
        this.PushAction = pushAction;
        this.UnavailabilityReason = unavailabilityReason;
    }
    public string Header { get; private set; }
    public string Text { get; private set; }
    public Action PushAction { get; private set; }

    public UnavailabilityReason UnavailabilityReason { get; private set; }

    public override T BuildView<T>(IUIContentViewBuilder<T> builder)
    {
        return builder.BuildButton(this);
    }

    public override string ProvideInfo => "Button Content provide info shouldn't be displayed";
}

public sealed class UnavailabilityReason
{
    public UnavailabilityReason(string reason)
    {
        Reason = reason;
    }
    public string Reason { get; private set; }
}