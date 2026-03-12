using System;

public static class TextConventionsHelper
{
    public static string GetColoredRarityText(string text, CarRarity rarity)
    {
        string colorHex = rarity switch
        {
            CarRarity.Common => "#FFFFFF",
            CarRarity.Rare => "#0070DD",
            CarRarity.Epic => "#A335EE",
            CarRarity.Legendary => "#FF8000",
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
        return "Placeholder stats:\n- Max Speed: 180\n- Acceleration: 5.0\n- Handling: 80";
    }
}