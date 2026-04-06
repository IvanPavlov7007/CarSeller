using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PrimarySelectionCarButtonWidgetCreator;

public class CarEntryView : WidgetView<CarEntryWidget>
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text CarName;
    [SerializeField] TMP_Text CarRarity;
    [SerializeField] Image typeImage;
    [SerializeField] AutoHidingButtonWidgetView AutoHidingButtonWidgetView;

    protected override RectTransform childrenContainer => null;

    protected override void Bind(CarEntryWidget widget)
    {
        setUpVisuals(widget);
        AutoHidingButtonWidgetView.Bind(widget.buttonWidget);
    }

    void setUpVisuals(CarEntryWidget widget)
    {
        image.sprite = widget.sprite;
        CarName.text = widget.name;
        CarRarity.text = widget.rarity;
        typeImage.sprite = widget.typeSprite;
    }
}

public class CarEntryWidget : Widget
{
    public Sprite sprite;
    public Sprite typeSprite;
    public string name;
    public string rarity;

    public AutoHidingButtonWidget buttonWidget;
    public string stateText;
    public Color? buttonColor;

    public CarEntryWidget(Car car, AutoHidingButtonWidget buttonWidget)
    {
        sprite = IconBuilderHelper.BuildProdutSpite(car);
        typeSprite = G.cityViewObjectBuilder.BuyersPinSprites[car.Kind.Type];
        name = car.Name;
        rarity = TextConventionsHelper.GetColoredRarityText(car.Kind.Rarity);
        this.buttonWidget = buttonWidget;
    }

    public CarEntryWidget Empty(AutoHidingButtonWidget buttonWidget)
    {
        sprite = null;
        typeSprite = null;
        name = "";
        rarity = "";
        this.buttonWidget = buttonWidget;
        return this;
    }
}

public static class  BuyCarButtonWidgetCreator
{
    static CarEntryWidget Create(PersonalVehicleShopEntry entry, Action onClick)
    {
        bool interactable = GameRules.CanBePurchased.Check(entry);
        string unavailabilityReason = interactable ? "" : GameRules.CanBePurchased.GetUnavailabilityReason(entry);

        var widget = new CarEntryWidget(entry.PersonalVehicle.Car, new AutoHidingButtonWidget(onClick, interactable, false, unavailabilityReason));

        return widget;
    }

    public static CarEntryWidget CreateBought(PersonalVehicleShopEntry entry, Action onClick)
    {
        var widget = Create(entry, onClick);
        widget.buttonWidget.IsInteractable = false;
        widget.buttonWidget.CloseParentMenuOnClick = true;
        widget.buttonWidget.UnavailabilityReason = "You already own this vehicle.";
        widget.stateText = "OWNED";
        return widget;
    }

    public static CarEntryWidget CreateAvailable(PersonalVehicleShopEntry entry, Action onClick)
    {
        var widget = Create(entry, onClick);
        widget.stateText = $"BUY: {TextConventionsHelper.FormatPrice(SellPriceWrapper.CalculateAbsolutePrice(entry.UnitPrice))}";
        return widget;
    }
}

public static class PrimarySelectionCarButtonWidgetCreator
{
    public static CarEntryWidget CreateAvailable(Car car, Action onClick)
    {
        var widget = new CarEntryWidget(car, new AutoHidingButtonWidget("Select",onClick, true, true));
        return widget;
    }

    public static CarEntryWidget CreateSelected(Car car, Action onClick)
    {
        var widget = new CarEntryWidget(car, new AutoHidingButtonWidget("SELECTED",onClick, true, true));
        widget.buttonColor = Color.gray;

        return widget;
    }

}