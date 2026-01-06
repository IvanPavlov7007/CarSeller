using System;

public class Collectable : CityObject
{
    //TODO make this betteer
    public Collectable(ILocation location, string name, float moneyAmount, IPossession[] possessions = null, Action additionalCallback = null)
        : base(name, moneyAmount.ToString(), location)
    {
        this.possessions = possessions;
        MoneyAmount = moneyAmount;
        OnCollectedAdditionalCallback += additionalCallback;
    }
    public IPossession[] possessions { get; set; }
    public float MoneyAmount { get; set; }
    public event Action OnCollectedAdditionalCallback;

    public void Collect()
    {
        Destroy();
        OnCollectedAdditionalCallback?.Invoke();
    }
}