using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Pixelplacement;
using System;

public class PrimarySelectionCarButtonView : WidgetView<PrimarySelectionCarButtonWidget>
{
    StateMachine stateMachine;
    protected override RectTransform childrenContainer => null;

    PrimarySelectionCarButtonState[] stateUIs;

    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
        stateUIs = GetComponentsInChildren<PrimarySelectionCarButtonState>(true);
    }

    protected override void Bind(PrimarySelectionCarButtonWidget widget)
    {
        foreach (var stateUI in stateUIs)
        {
            stateUI.SetUp(widget);
        }
        stateMachine.ChangeState((int)widget.CurrentSelectionState);
    }
}



public class PrimarySelectionCarButtonWidget : ButtonWidget
{
    public readonly Car Car;
    public readonly SelectionState CurrentSelectionState;
    public readonly Action OnClick;

    public readonly Sprite sprite;
    public readonly string name;
    public readonly string rarity;
    public readonly string description;

    public PrimarySelectionCarButtonWidget(Car car, SelectionState selectionState, Action onClick)
        : base(onClick)
    {
        this.Car = car;
        this.CurrentSelectionState = selectionState;

        sprite = IconBuilderHelper.BuildProdutSpite(car);
        name = car.Name;
        rarity = TextConventionsHelper.GetColoredRarityText(car.Rarity);
        description = TextConventionsHelper.CarDescription(car);
        OnClick = onClick;
    }


    [Serializable]
    public enum SelectionState
    {
        Available,
        Selected,
        Unavailable
    }

}