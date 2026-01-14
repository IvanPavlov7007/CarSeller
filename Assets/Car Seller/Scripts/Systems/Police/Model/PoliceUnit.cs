//TODO Maybe make police be more than just a city object in the future
using System;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

using CityPosition = City.CityPosition;
using Random = UnityEngine.Random;

public class PoliceUnit : CityObject, PoliceUnitAIData
{
    public City.CityPosition CityPosition => Location.CityPosition;
    public Vector2 TargetPosition { get; set; }
    public IAIMovement Movement => GraphMovement;
    public IVision Vision => ConeVision;
    public PoliceUnitState State { get; set; }
    public PersonalityTag Personality{get; private set; }
    public ConeVision ConeVision { get; private set; }
    public GraphMovement GraphMovement { get; private set; }

    public PoliceUnit(string name, string infoText, ILocation location, City.CityMarker cityMarker,
        SpeedVarations speedVariations, float visionRadius, float visionAngle, PersonalityTag personality,
        PinStyle pinStyle = null) : base(name, infoText, location, cityMarker, pinStyle)
    {
        GraphMovement = new GraphMovement(this,speedVariations);
        ConeVision = new ConeVision(this, visionRadius, visionAngle);
        Personality = personality;
        randomizeInitialDirection();
    }

    private void randomizeInitialDirection()
    {
        var positionData = CityPosition;
        if(positionData.Edge != null && positionData.Edge.Bidirectional && Random.value < 0.5f)
        {
            positionData.Edge.Bidirectional = !positionData.Edge.Bidirectional;
        }
        Location.SetCityPosition(positionData);
    }

    internal void Update(float deltaTime, PoliceAISystem aiSystem, PoliceAIStateMachine stateMachine)
    {
        var positionData = CityPosition;
        RoadNode a = positionData.Node;//Anchor node - node from which we move from
        RoadEdge edge = positionData.Edge;
        bool forward = positionData.Forward;
        float t = positionData.Percentage;
        Vector2 chosenTangentDirection;

        //First check if we exactly on a node
        if (a != null)
        {
            //Check if there is an edge in the desired direction
            edge = aiSystem.PickTurn(stateMachine, this, a, edge, stateMachine.SuspectLastSeenEdge);
            
            if (edge != null)
            {
                chosenTangentDirection = edge.GetTangentFromNode(a, 0f, out forward);
            }
            else
            {
                return; // No edge to go to
            }
        }
        else
        {
            //We are between nodes, need to understand which direction

            if (forward)
            {
                a = edge.From;
            }
            else
            {
                a = edge.To;
            }
        }

        float stepLength = deltaTime * GraphMovement.Speed;
        float length_a_to_b = edge.Length;


        //water flow in a noded network
        while (stepLength >= length_a_to_b * (1f - t))// While step is longer than remaining distance to B
        {
            stepLength -= length_a_to_b * (1f - t);
            a = (a == edge.From) ? edge.To : edge.From;
            var newEdge = aiSystem.PickTurn(stateMachine, this, a, edge, stateMachine.SuspectLastSeenEdge);
            if (newEdge == null)
            {
                //No edge to proceed further, stop at the current node
                stepLength = 0f;
                break;
            }

            length_a_to_b = edge.Length;
            t = 0f;
            forward = (newEdge.From == a);
        }

        if (edge == null)
        {
            //We have reached the end of the line, stop at node A
            positionData = CityPosition.At(a);
        }
        else
        {
            t = (t * length_a_to_b + stepLength) / length_a_to_b;

            positionData = CityPosition.On(edge, t, forward);
        }
        Location.SetCityPosition(positionData);
    }


}

public class GraphMovement : IAIMovement, ISpeedProvider
{
    public bool TowardsTarget { get; set; }

    public PoliceUnit Owner { get; set; }
    public GraphMovement(PoliceUnit owner, SpeedVarations speedVarations)
    {
        SpeedVarations = speedVarations;
        Owner = owner;
    }

    public TempoState TempoState { get; set; }
    Vector2 up = Vector2.up;
    public Vector2 Up
    {
        get
        {
            var edge = Owner.CityPosition.Edge;
            if (edge == null)
            {
                return up;
            }
            
            var tangent = Owner.CityPosition.Edge.GetTangentFromNode(edge.From,
                Owner.CityPosition.Percentage, out bool forward);
            return forward == Owner.CityPosition.Forward ? tangent : -tangent;
        }
    }

    public SpeedVarations SpeedVarations { get; set; }

    public float Speed {
        get {         
            return TempoState switch
            {
                TempoState.Slow => SpeedVarations.Slow,
                TempoState.Medium => SpeedVarations.Medium,
                TempoState.Fast => SpeedVarations.Fast,
                _ => SpeedVarations.Medium,
            };
        }
    }
    //Currently ignored
    public float Acceleration { get; set; }
}

[Serializable]
public struct SpeedVarations
{
    public float Slow;
    public float Medium;
    public float Fast;
}

public class ConeVision : IVision
{
    public readonly PoliceUnit Owner;
    public float Radius;
    /// <summary>
    /// Full cone angle in degrees.
    /// </summary>
    public float Angle;

    public ConeVision(PoliceUnit owner, float radius, float angle)
    {
        Owner = owner;
        Radius = radius;
        Angle = angle;
    }

    public bool CanSeeTarget(City.CityPosition target)
    {
        var ownerPos = Owner.CityPosition.WorldPosition;
        var ownerUp = Owner.GraphMovement.Up;
        var targetPos = target.WorldPosition;

        // 1. Vector from owner to target
        Vector2 toTarget = targetPos - ownerPos;

        // 2. Distance check
        float distanceSqr = toTarget.sqrMagnitude;
        float radiusSqr = Radius * Radius;
        if (distanceSqr > radiusSqr)
            return false;

        // 3. Degenerate check
        if (toTarget.sqrMagnitude < Mathf.Epsilon || ownerUp.sqrMagnitude < Mathf.Epsilon)
            return false;

        // 4. Normalize
        Vector2 dirToTarget = toTarget.normalized;
        Vector2 forward = ownerUp.normalized;

        // 5. Angle check via dot product
        // cos(theta) = dot(a, b) when a and b are normalized.
        float dot = Vector2.Dot(forward, dirToTarget);

        // Precompute cosine of half-angle
        float halfAngleRad = (Angle * 0.5f) * Mathf.Deg2Rad;
        float cosHalfAngle = Mathf.Cos(halfAngleRad);

        // If dot >= cos(halfAngle), target is inside the cone.
        return dot >= cosHalfAngle;
    }
}