using System;
using System.Collections.Generic;
using UnityEngine;

public class PersonalVehicleShop : ILocatable
{
    NonDuplicatesIdentifiedCarList<PersonalVehicleShopEntry> AllPersonalVehiclesShopEntries;
    PersonalVehiclesList list;

    public PersonalVehiclesList PersonalVehiclesList => list;

    public PersonalVehicleShop(VehicleShopConfig shopConfig)
    {
        AllPersonalVehiclesShopEntries = new NonDuplicatesIdentifiedCarList<PersonalVehicleShopEntry>();
        foreach (var carIdentifier in shopConfig.allCarOptions)
        {
            AllPersonalVehiclesShopEntries.Add(carIdentifier, new PersonalVehicleShopEntry(carIdentifier, this));
        }
        GeneratePersonalVehiclesList(shopConfig.initiallyUnlockedOptions);
    }

    private void GeneratePersonalVehiclesList(IReadOnlyList<SimplifiedCarIdentifier> initiallyAvailable)
    {
        Debug.Assert(initiallyAvailable != null, "Initially available cars list cannot be null.");
        Debug.Assert(AllPersonalVehiclesShopEntries != null);

        var pickedEntries = new List<PersonalVehicleShopEntry>();
        foreach (var carIdentifier in initiallyAvailable)
        {
            if (!AllPersonalVehiclesShopEntries.TryGetValue(carIdentifier, out var entry))
            {
                throw new UnityException($"Attempted to generate a personal vehicles list with an initially available car identifier {carIdentifier} that does not exist in the shop entries.");
            }
            pickedEntries.Add(entry);
        }
        unlockEntries(pickedEntries);

        list = new PersonalVehiclesList(createPersonalVehiclesFromUnlockedEntries());
    }

    public IReadOnlyList<PersonalVehicleShopEntry> GetAvailableEntries()
    {
        var availableEntries = new List<PersonalVehicleShopEntry>();
        foreach (var entry in AllPersonalVehiclesShopEntries.Values)
        {
            if (entry.IsAvailable)
            {
                availableEntries.Add(entry);
            }
        }
        return availableEntries;
    }
    private IReadOnlyList<PersonalVehicle> createPersonalVehiclesFromUnlockedEntries()
    {
        var personalVehicles = new List<PersonalVehicle>();
        foreach (var entry in AllPersonalVehiclesShopEntries.Values)
        {
            if(entry.IsUnlocked)
                personalVehicles.Add(entry.PersonalVehicle);
        }
        return personalVehicles;
    }

    private void unlockEntries(List<PersonalVehicleShopEntry> entries)
    {
        foreach (var entry in entries)
        {
            entry.Unlock();
        }
    }

    public void UpdateList()
    {
        list.UpdateList(createPersonalVehiclesFromUnlockedEntries());
    }

}

public class PersonalVehicleShopEntry : IPurchasable
{
    PersonalVehicle personalVehicle;
    PersonalVehicleShop shop;
    bool unlocked = false;

    public bool IsUnlocked => unlocked;
    public bool IsAvailable { get; private set; } = true;
    public PersonalVehicle PersonalVehicle => personalVehicle;

    internal PersonalVehicleShopEntry(SimplifiedCarIdentifier carIdentifier, PersonalVehicleShop shop)
    {
        personalVehicle = PersonalVehicle.CreateNew(carIdentifier);
        Price = getPrice();
        this.shop = shop;
    }
    public float Price { get; private set; }

    public void CompletePurchase()
    {
        UnlockAndUpdateShop();
    }

    internal void Unlock()
    {
        unlocked = true;
    }

    void UnlockAndUpdateShop()
    {
        Unlock();
        shop.UpdateList();
    }

    public bool TryPurchase(TransactionFeedbackLocation feedbackLocation)
    {
        var transaction = new PurchaseTransaction(Price, this);
        var result = G.TransactionProcessor.Process(transaction,feedbackLocation);
        if(result.Type == TransactionResultType.Success)
        {
            return true;
        }
        Debug.LogError($"Failed to purchase personal vehicle {personalVehicle.Car.Name}. Transaction result: {result}");
        return false;
    }

    private float getPrice()
    {
        return personalVehicle.Car.CalculatePrice();
    }
}


public class PersonalVehicleShopController
{
    PersonalVehicleShop shop;

    public event Action onRefreshUI;

    public PersonalVehicleShopController(PersonalVehicleShop shop, Action onRefresh)
    {
        this.shop = shop;
        onRefreshUI += onRefresh;
    }

    public UIElement GenerateMenuUI()
    {
        var container = new UIElement()
        {
            Type = UIElementType.Container,
            Style = "grid",
            Children = new List<UIElement>()
        };

        populateContainerWithSlots(container, shop.GetAvailableEntries());
        return container;
    }

    private void populateContainerWithSlots(UIElement container, IReadOnlyList<PersonalVehicleShopEntry> AvailableShopEntries)
    {
        var children = container.Children;
        foreach (var item in AvailableShopEntries)
        {
            if (item.IsUnlocked)
            {
                children.Add(ownedEntry(item));
            }
            else
            {
                children.Add(availableEntry(item));
            }
        }
    }

    private UIElement ownedEntry(PersonalVehicleShopEntry entry)
    {
        return new UIElement()
        {
            Type = UIElementType.ButtonContainer,
            IsInteractable = false,
            Children = new List<UIElement>()
            {
                CTX_Menu_Tools.CarIcon(entry.PersonalVehicle.Car),
                CTX_Menu_Tools.CarRarityText(entry.PersonalVehicle.Car),
                CTX_Menu_Tools.Header("Purchased")
            }
        };
    }

    private UIElement availableEntry(PersonalVehicleShopEntry entry)
    {
        return new UIElement()
        {
            Type = UIElementType.ButtonContainer,
            OnClick = () => entryClicked(entry),
            IsInteractable = GameRules.CanBePurchased.Check(entry),
            UnavailabilityReason = "Not enough money!",
            Children = new List<UIElement>()
            {
                CTX_Menu_Tools.CarIcon(entry.PersonalVehicle.Car),
                CTX_Menu_Tools.CarRarityText(entry.PersonalVehicle.Car),
                CTX_Menu_Tools.Header($"Price: {entry.Price}")
            }
        };
    }

    private void entryClicked(PersonalVehicleShopEntry entry)
    {
        PurchaseEntry(entry);
    }


    /// <summary>
    /// Part where we orchestrate the change in game
    /// </summary>
    /// <param name="entry"></param>
    private void PurchaseEntry(PersonalVehicleShopEntry entry)
    {
        if (!entry.TryPurchase(TransactionFeedbackLocation.OmniDirectional))
            return;
        G.VehicleController.SwapPrimaryVehicle(entry.PersonalVehicle);
        onRefreshUI?.Invoke();
    }

    public UIElement GenerateInfoUI(PersonalVehicleShopEntry entry)
    {
        throw new NotImplementedException();
    }

}

public class PurchaseTransaction : Transaction
{
    public PurchaseTransaction(float price, params IPurchasable[] purchases)
    {
        Price = price;
        this.Purchases = purchases;
    }

    public readonly float Price;
    public readonly IReadOnlyList<IPurchasable> Purchases;
}

public interface IPurchasable
{
    public float Price { get; }
    public bool IsAvailable { get; }
    public void CompletePurchase();
}