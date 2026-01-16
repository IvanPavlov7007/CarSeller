using UnityEngine;

[CreateAssetMenu(fileName = "PhysicalProductViewBuilder", menuName = "Configs/View/PhysicalProductViewBuilder")]
public class PhysicalProductGameObjectBuilder : WarehouseProductGameObjectBuilder
{
    [Header("Physics settings")]
    public float carMass = 1f;
    public float wheelMass = 0.5f;
    public bool enableWheelMotor = false;
    public float wheelMotorSpeed = 0f;
    public float wheelMotorMaxTorque = 0f;
    public float wheelDampingRatio = 0.5f;
    public float wheelFrequency = 5f;

    public override GameObject BuildCar(Car car)
    {
        // Root for the physical car
        var carGO = new GameObject(car.Name);
        var carRb = carGO.AddComponent<Rigidbody2D>();
        carRb.mass = carMass;

        // FRAME (visual)
        var frameGO = car.CarFrame.GetRepresentation(carPartViewBuilder);
        frameGO.transform.SetParent(carGO.transform, worldPositionStays: false);
        frameGO.transform.localPosition = Vector3.zero;
        frameGO.transform.localRotation = Quaternion.identity;

        // BUILD PHYSICAL PARTS
        foreach (var partLocation in car.carParts.Keys)
        {
            var slotData = partLocation.PartSlotRuntimeConfig.partSlotData;

            // If not occupied, skip
            if (slotData.Hidden || partLocation.Occupant == null)
                continue;

            if (partLocation.Occupant is Wheel wheelProduct)
            {
                // First build & place the wheel using the shared helper
                var wheelGO = CarPartViewPlacementHelper.BuildCarPartAtPosition(
                    partLocation,
                    carGO.transform,
                    carPartViewBuilder);

                if (wheelGO != null)
                {
                    SetupPhysicalWheel(wheelGO, wheelProduct, partLocation, carRb);
                }
            }
            else
            {
                // Non-wheel parts use the same helper for placement
                CarPartViewPlacementHelper.BuildCarPartAtPosition(
                    partLocation,
                    carGO.transform,
                    carPartViewBuilder);
            }
        }

        // Attach warehouse-level view/drag logic to the whole car (not to individual parts)
        productViewComponentBuilder.BuildViewComponent(carGO, car);

        return carGO;
    }

    public override GameObject BuildCarFrame(CarFrame carFrame)
    {
        Debug.LogError("PhysicalProductGameObjectBuilder does not support building standalone CarFrame views. " + carFrame.UniqueName);
        return null;
    }

    /// <summary>
    /// Adds physics and joints to an already built wheel GameObject.
    /// The wheel is assumed to be correctly parented and positioned by CarPartViewPlacementHelper.
    /// </summary>
    private void SetupPhysicalWheel(
        GameObject wheelGO,
        Wheel wheel,
        Car.CarPartLocation partLocation,
        Rigidbody2D carRb)
    {
        var slotData = partLocation.PartSlotRuntimeConfig.partSlotData;

        // Ensure rigidbody on wheel
        var wheelRb = wheelGO.GetComponent<Rigidbody2D>();
        if (wheelRb == null)
        {
            wheelRb = wheelGO.AddComponent<Rigidbody2D>();
        }
        wheelRb.mass = wheelMass;

        // Attach wheel to car with a WheelJoint2D
        var joint = wheelGO.AddComponent<WheelJoint2D>();
        joint.connectedBody = carRb;
        joint.autoConfigureConnectedAnchor = false;

        // Anchors: wheel's own center as localAnchor; car-local anchor same as slot local position
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = slotData.LocalPosition;

        var suspension = joint.suspension;
        suspension.dampingRatio = wheelDampingRatio;
        suspension.frequency = wheelFrequency;
        joint.suspension = suspension;

        joint.useMotor = enableWheelMotor;
        if (enableWheelMotor)
        {
            var motor = joint.motor;
            motor.motorSpeed = wheelMotorSpeed;
            motor.maxMotorTorque = wheelMotorMaxTorque;
            joint.motor = motor;
        }

        // IMPORTANT: no productViewComponentBuilder.BuildViewComponent here,
        // wheels visually/semantically belong to the car in the warehouse.
    }
}