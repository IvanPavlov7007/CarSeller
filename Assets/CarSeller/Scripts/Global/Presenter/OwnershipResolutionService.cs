using System.Collections.Generic;
using UnityEngine;

public class OwnershipResolutionService
{
    private Dictionary<IOwnable, HashSet<IOwnable>> ownerships => World.Instance.ownerships;

    public void RegisterProduct(OwnableBase ownable)
    {
        Debug.Assert(ownable != null);

        // Avoid double-subscribing if RegisterProduct is called multiple times.
        ownable.OnOwnerChanged -= onOwnerChanged;
        ownable.OnOwnerChanged += onOwnerChanged;

        // Ensure current owner relationship exists in the index.
        // (Useful if you load saved games / spawn with pre-set owners.)
        onOwnerChanged(null, ownable.Owner, ownable);
    }

    public bool tryResolveOwnership(IMutableOwnable ownable, ILocation location)
    {
        if (ownable == null || location == null)
            return false;

        // Holder is typically Warehouse, HiddenSpace, CarPark, etc.
        if (location.Holder is IOwnershipContainer container)
        {
            ResolveOnPlacement(ownable, container);
            return true;
        }

        // Location doesn't participate in ownership resolution
        return true;
    }

    public void ResolveOnPlacement(IMutableOwnable item, IOwnershipContainer container)
    {
        Debug.Assert(item != null);
        Debug.Assert(container != null);

        switch (container.OwnershipResolution)
        {
            case OwnershipResolution.Container:
                item.SetOwner(container as IOwnable);
                break;

            case OwnershipResolution.OwnerOfContainerIfNull:
                if (item.Owner == null)
                    item.SetOwner(container.GetOwnerOfContainer());
                break;

            case OwnershipResolution.Clear:
                item.SetOwner(null);
                break;

            case OwnershipResolution.None:
                // Do nothing
                break;
        }
    }

    public void TransferOwnership(IMutableOwnable item, IOwnable newOwner)
    {
        Debug.Assert(item != null);
        item.SetOwner(newOwner);
    }

    public void ResolveOnRemoval(IMutableOwnable item)
    {
        Debug.Assert(item != null);
        item.SetOwner(null);
    }

    private void onOwnerChanged(IOwnable oldOwner, IOwnable newOwner, IOwnable item)
    {
        if (ReferenceEquals(oldOwner, newOwner))
            return;

        if (oldOwner != null && ownerships.TryGetValue(oldOwner, out var oldSet))
        {
            oldSet.Remove(item);
            if (oldSet.Count == 0)
                ownerships.Remove(oldOwner);
        }

        if (newOwner != null)
        {
            if (!ownerships.TryGetValue(newOwner, out var newSet))
            {
                newSet = new HashSet<IOwnable>();
                ownerships[newOwner] = newSet;
            }
            newSet.Add(item);
        }

        // Optional: add a new GameEvent that UI can listen to instead of "Possession acquired".
        GameEvents.Instance.OnOwnershipChanged?.Invoke(new OwnershipChangedEventData(item, oldOwner, newOwner));
    }
}

public class OwnershipService
{
    public void TransferOwnership(IMutableOwnable item, IOwnable newOwner)
    {
        G.ProductLifetimeService.TransferOwnership(item, newOwner);
    }
}