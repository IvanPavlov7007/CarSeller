using System;
using System.Globalization;

public static class TextConventionsHelper
{
    public static string GetColoredRarityText(string text, CarRarity rarity)
    {
        string colorHex = rarity switch
        {
            CarRarity.Common => "#FFFFFF",
            CarRarity.Uncommon => "#0070DD",
            CarRarity.Epic => "#A335EE",
            //CarRarity.Legendary => "#FF8000",
            _ => "#FFFFFF"
        };
        return $"<color={colorHex}>{text}</color>";
    }

    public static string GetColoredRarityText(CarRarity rarity)
    {
        return GetColoredRarityText(rarity.ToString(), rarity);
    }

    public static string CarDescription(Car car)
    {
        return $"Type: {car.Type.ToString()}\nColor: {car.Color.ToString()}";
    }

    public static string FormatPrice(float price)
    {
        return price.ToString("C0", CultureInfo.CurrentCulture);
    }

    public static string FormatPriceWithSign(float price)
    {
        string numbs = FormatPrice(Math.Abs(price));
        if (price > 0)
        {
            return $"+{numbs}";
        }
        else if (price < 0)
        {
            return $"-{numbs}";
        }
        else
        {
            return numbs;
        }
    }
}