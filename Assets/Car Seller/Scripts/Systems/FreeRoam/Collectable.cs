using System;

public class Collectable
{
    public string Name { get; set; }
    public IPossession[] possessions { get; set; }
    public float MoneyAmount { get; set; }
    public event Action OnCollectedAdditionalCallback;
    public void Collect()
    {
        OnCollectedAdditionalCallback?.Invoke();
    }
}