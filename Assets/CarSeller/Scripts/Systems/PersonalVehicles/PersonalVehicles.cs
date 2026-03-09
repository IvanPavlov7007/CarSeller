using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersonalVehicles : ILocationsHolder
{
    public ILocation PrimaryVehicleLocation { get; private set; }
    public Car PrimaryVehicle =>
        PrimaryVehicleLocation != null ? (PrimaryVehicleLocation as PersonalVehicleLocation)?.Occupant as Car : null;
    public List<PersonalVehicleLocation> OwnedVehicles;

    public PersonalVehicles(int maxCount)
    {
        OwnedVehicles = new List<PersonalVehicleLocation>(maxCount);
    }

    public bool SetPrimaryVehicle(Car car)
    {
        var foundLocation = OwnedVehicles.FindIndex(x=>x.Occupant == car);
        if( foundLocation < 0 || foundLocation >= OwnedVehicles.Count)
        {
            Debug.LogWarning("Attempted to set a primary vehicle that is not owned.");
            return false;
        }
        return SetPrimaryVehicle(foundLocation);
    }

    public bool SetPrimaryVehicle(int index)
    {
        if(index < 0 || index >= OwnedVehicles.Count)
        {
            Debug.LogWarning("Attempted to set a primary vehicle with an invalid index.");
            return false;
        }
        PrimaryVehicleLocation = OwnedVehicles[index];
        GameEvents.Instance.onPrimaryVehicleChanged?.Invoke(PrimaryVehicle);
        return true;
    }

    public ILocation[] GetLocations()
    {
        return OwnedVehicles.Select(x=> x as ILocation).ToArray();
    }
    public class PersonalVehicleLocation : ILocation
    {
        public Car Car { get; private set; }
        public PersonalVehicles PersonalVehicles { get; private set; }
        public ILocatable Occupant => Car;
        public ILocationsHolder Holder => PersonalVehicles;

        public PersonalVehicleLocation(PersonalVehicles personalVehicles)
        {
            this.PersonalVehicles = personalVehicles;
        }

        public bool Attach(ILocatable product)
        {
            Debug.Assert(product != null);
            Debug.Assert(this.Car == null, "PersonalVehicleLocation can only hold one car at a time.");
            Debug.Assert(product is Car, "PersonalVehicleLocation can only hold products of type Car.");

            if (product is Car car)
            {
                this.Car = car;
                return true;
            }
            return false;
        }

        public void Detach()
        {
            this.Car = null;
        }
    }

}