using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PrimarySelectionCarButtonWidgetCreator;

public class CarButtonView : ButtonView<CarButtonWidget>
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text CarName;
    [SerializeField] TMP_Text CarRarity;
    [SerializeField] TMP_Text CarDescription;
    [SerializeField] TMP_Text StateText;

    Image backgroundImage;

    protected override RectTransform childrenContainer => null;

    private void Initialize()
    {
        backgroundImage = GetComponent<Image>();
    }

    protected override void Bind(CarButtonWidget widget)
    {
        base.Bind(widget);
        Initialize();
        setUpVisuals(widget);
    }

    void setUpVisuals(CarButtonWidget widget)
    {
        image.sprite = widget.sprite;
        CarName.text = widget.name;
        CarRarity.text = widget.rarity;
        CarDescription.text = widget.description;

        backgroundImage.color = widget.buttonColor ?? Color.white;
        StateText.text = widget.stateText;
        if(string.IsNullOrEmpty(widget.stateText))
            StateText.gameObject.SetActive(false);
    }
}

public class CarButtonWidget : ButtonWidget
{
    public Sprite sprite;
    public string name;
    public string rarity;
    public string description;

    public string stateText;
    public Color? buttonColor;

    public CarButtonWidget(Car car, Action onClick, bool isInteractable = true, bool closeParentMenuOnClick = false, string unavailabilityReason = "")
         : base(onClick, isInteractable,closeParentMenuOnClick, unavailabilityReason)
    {
        sprite = IconBuilderHelper.BuildProdutSpite(car);
        name = car.Name;
        rarity = TextConventionsHelper.GetColoredRarityText(car.Rarity);
        description = TextConventionsHelper.CarDescription(car);
        OnClick = onClick;
    }
}

public static class  BuyCarButtonWidgetCreator
{
    static CarButtonWidget Create(PersonalVehicleShopEntry entry, Action onClick)
    {
        bool interactable = GameRules.CanBePurchased.Check(entry);
        string unavailabilityReason = interactable ? "" : GameRules.CanBePurchased.GetUnavailabilityReason(entry);

        var widget = new CarButtonWidget(entry.PersonalVehicle.Car, onClick,interactable,false,unavailabilityReason);

        return widget;
    }

    public static CarButtonWidget CreateBought(PersonalVehicleShopEntry entry, Action onClick)
    {
        var widget = Create(entry, onClick);
        widget.IsInteractable = false;
        widget.CloseParentMenuOnClick = true;
        widget.UnavailabilityReason = "You already own this vehicle.";
        widget.stateText = "OWNED";
        return widget;
    }

    public static CarButtonWidget CreateAvailable(PersonalVehicleShopEntry entry, Action onClick)
    {
        var widget = Create(entry, onClick);
        widget.stateText = $"BUY: {TextConventionsHelper.FormatPrice(entry.Price)}";
        return widget;
    }
}

public static class PrimarySelectionCarButtonWidgetCreator
{
    public static ButtonWidget CreateAvailable(Car car, Action onClick)
    {
        var widget = new CarButtonWidget(car, onClick, true, true);
        return widget;
    }

    public static ButtonWidget CreateSelected(Car car, Action onClick)
    {
        var widget = new CarButtonWidget(car, onClick, true, true);
        widget.buttonColor = Color.gray;
        widget.stateText = "SELECTED";

        return widget;
    }

}