using System.Collections.Generic;
using UnityEngine;
public interface IOffer
{
    public abstract Transaction Accept();
    public abstract bool CanAccept();
}

public interface IOfferProvider<T> where T : IOffer
{
    List<T> GetOffers();
    void OfferAccepted(T offer);
}

//public abstract class OfferBase<T> : IOffer where T:IOffer
//{
//    protected OfferProviderBase<T> owner;
//    public abstract Transaction Accept();
//    public abstract bool CanAccept();
//}

//public abstract class OfferProviderBase<T> : IOfferProvider<T> where T : IOffer
//{
//    protected List<T> offers;
//    public abstract List<T> GetOffers();
//    public void OfferAccepted(T offer)
//    {
//        Debug.Assert(offer != null);
//        offers.Remove(offer);

//    }
//}