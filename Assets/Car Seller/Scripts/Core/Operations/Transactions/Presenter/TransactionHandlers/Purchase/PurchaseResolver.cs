
using UnityEngine;

public class PurchaseResolver
{
    public void ResolvePurchase(IMutableOwnable[] purchasedItems)
    {
        foreach (var item in purchasedItems)
        {
            ResolvePurchase(item);
        }
    }

    public void ResolvePurchase(IMutableOwnable purchasedItem)
    {
        switch(purchasedItem)
        {
            case Warehouse warehouse:
                // When purchasing a warehouse, set its owner to the player.
                purchasedItem.SetOwner(G.Player);
                break;

            case Car car:
            case Product product:
            default:
                Debug.LogWarning($"PurchaseResolver: No ownership resolution defined for purchased item of type {purchasedItem.GetType().Name}");
                break;
        }
    }
}