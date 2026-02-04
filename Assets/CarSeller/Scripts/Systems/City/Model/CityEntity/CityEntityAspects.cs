using System;

public interface CityEntityAspect { }

public sealed class MarkerReferenceAspect : CityEntityAspect
{
    public readonly City.CityMarker CityMarker;

    public MarkerReferenceAspect(City.CityMarker cityMarker)
    {
        CityMarker = cityMarker;
    }
}
public sealed class PinStyleAspect : CityEntityAspect
{
    public readonly PinStyle Style;

    public PinStyleAspect(PinStyle style)
    {
        Style = style ?? throw new ArgumentNullException(nameof(style));
    }
}
public sealed class TriggerableAspect : CityEntityAspect
{
}

public class InteractableAspect : CityEntityAspect
{
    public readonly int SortingOrder;

    public InteractableAspect(int sortingOrder)
    {
        SortingOrder = sortingOrder;
    }
}

public sealed class DragInteractableAspect : InteractableAspect
{
    public DragInteractableAspect(int sortingOrder) : base(sortingOrder)
    {
    }
}

public sealed class CarAspect : CityEntityAspect
{
}

public sealed class PoliceUnitAspect : CityEntityAspect
{
}

public sealed class RigidbodyAspect : CityEntityAspect
{

}