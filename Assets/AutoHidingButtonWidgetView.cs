using System;
using TMPro;
using UnityEngine;

public class AutoHidingButtonWidgetView : ButtonView<AutoHidingButtonWidget>
{
    public TMP_Text ButtonText;

    protected override RectTransform childrenContainer => null;

    protected override void Bind(AutoHidingButtonWidget widget)
    {
        base.Bind(widget);
        ButtonText.text = widget.ButtonText;
        if (widget.OnClick == null)
        {
            gameObject.SetActive(false);
        }
    }
}

public class AutoHidingButtonWidget : ButtonWidget
{
    public string ButtonText;
    public AutoHidingButtonWidget(string ButtonText, Action onClick, bool isInteractable = true, bool closeParentMenuOnClick = false, string unavailabilityReason = "") : base(onClick, isInteractable, closeParentMenuOnClick, unavailabilityReason)
    {
        this.ButtonText = ButtonText;
    }

    public AutoHidingButtonWidget(Action onClick, bool isInteractable = true, bool closeParentMenuOnClick = false, string unavailabilityReason = "") : base(onClick, isInteractable, closeParentMenuOnClick, unavailabilityReason)
    {
    }
}