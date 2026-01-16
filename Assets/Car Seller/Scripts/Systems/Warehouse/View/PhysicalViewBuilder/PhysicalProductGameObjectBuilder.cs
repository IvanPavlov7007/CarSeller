using System.Collections.Generic;
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

    [Header("Layer settings")]
    [Tooltip("Layer index used for the assembled car (body + wheels).")]
    public int carLayer = 0; // set this in the inspector to e.g. a 'Car' layer

    public override GameObject BuildCar(Car car)
    {
        // Root for the physical car
        var carGO = new GameObject(car.Name);
        carGO.layer = carLayer;

        var carRb = carGO.AddComponent<Rigidbody2D>();
        carRb.mass = carMass;

        // FRAME (visual)
        var frameGO = car.CarFrame.GetRepresentation(carPartViewBuilder);
        frameGO.transform.SetParent(carGO.transform, worldPositionStays: false);
        frameGO.transform.localPosition = Vector3.zero;
        frameGO.transform.localRotation = Quaternion.identity;
        SetLayerRecursively(frameGO, carLayer);

        // cache all colliders on the car body hierarchy for later ignore-collision setup
        var carColliders = new List<Collider2D>();
        carColliders.AddRange(carGO.GetComponentsInChildren<Collider2D>());

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
                    SetLayerRecursively(wheelGO, carLayer);
                    SetupPhysicalWheel(wheelGO, wheelProduct, partLocation, carRb, carColliders);
                }
            }
            else
            {
                // Non-wheel parts use the same helper for placement
                var partGO = CarPartViewPlacementHelper.BuildCarPartAtPosition(
                    partLocation,
                    carGO.transform,
                    carPartViewBuilder);

                if (partGO != null)
                {
                    SetLayerRecursively(partGO, carLayer);
                    // keep colliders for later ignore-collision setup if needed
                    carColliders.AddRange(partGO.GetComponentsInChildren<Collider2D>());
                }
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
    /// Also makes the wheel ignore collisions with other colliders in the same car.
    /// </summary>
    private void SetupPhysicalWheel(
        GameObject wheelGO,
        Wheel wheel,
        Car.CarPartLocation partLocation,
        Rigidbody2D carRb,
        List<Collider2D> carColliders)
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

        // ----- COLLISION FILTERING -----
        // Get all colliders on this wheel
        var wheelColliders = wheelGO.GetComponentsInChildren<Collider2D>();
        foreach (var wCol in wheelColliders)
        {
            if (wCol == null) continue;

            // Ignore collisions between this wheel and all other colliders in the same car
            foreach (var cCol in carColliders)
            {
                if (cCol == null || cCol == wCol) continue;
                Physics2D.IgnoreCollision(wCol, cCol, true);
            }

            // Also add wheel's own colliders to the carColliders list so
            // subsequent wheels can ignore them too.
            if (!carColliders.Contains(wCol))
                carColliders.Add(wCol);
        }

        // IMPORTANT: no productViewComponentBuilder.BuildViewComponent here,
        // wheels visually/semantically belong to the car in the warehouse.
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}