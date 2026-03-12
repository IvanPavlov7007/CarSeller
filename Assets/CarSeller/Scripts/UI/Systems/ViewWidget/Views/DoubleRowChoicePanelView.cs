using TMPro;
using UnityEngine;

public class DoubleRowChoicePanelView : WidgetView<DoubleRowChoicePanelWidget>
{
    [SerializeField] TMP_Text header;
    [SerializeField] TMP_Text description;
    [SerializeField] RectTransform contentRoot;

    protected override RectTransform childrenContainer => contentRoot;

    protected override void Bind(DoubleRowChoicePanelWidget widget)
    {
        header.text = widget.Header;
        description.text = widget.Description;
    }
}

public class DoubleRowChoicePanelWidget : BlockingInputWidget
{
        public readonly string Header;
        public readonly string Description;

    public DoubleRowChoicePanelWidget(string header, string description)
    {
        Header = header;
        Description = description;
    }
}