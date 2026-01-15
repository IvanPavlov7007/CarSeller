using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using CityPosition = City.CityPosition;
using Random = UnityEngine.Random;
public class PoliceAISystem
{
    public void UpdateState(PoliceAIContext state)
    {
        checkForSuspect(state);
        updateUnits(state);
    }

    void checkForSuspect(PoliceAIContext stateMachine)
    {
        var unitsSeeingSuspect = new List<PoliceUnitAIData>();
        var tryingToRelocateUnits = new List<PoliceUnitAIData>();
        var lastSeenPositionReached = false;

        foreach (var unit in stateMachine.PoliceUnits)
        {
            if (unit.Vision.CanSeeTarget(stateMachine.SuspectRealPosition))
            {
                unitsSeeingSuspect.Add(unit);
            }
        }

        if (unitsSeeingSuspect.Count > 0)
        {
            // Suspect visible: Chase/Backup
            foreach (var unit in stateMachine.PoliceUnits)
            {
                unit.State = unitsSeeingSuspect.Contains(unit)
                    ? PoliceUnitState.Chase
                    : PoliceUnitState.Backup;
            }

            updateLastSeenPosition(stateMachine, stateMachine.SuspectRealPosition);
            stateMachine.State = PoliceAISystemState.Chase;
            return;
        }

        // No one sees the suspect: TryRelocate / Calm handling
        foreach (var unit in stateMachine.PoliceUnits)
        {
            switch (unit.State)
            {
                case PoliceUnitState.Chase:
                case PoliceUnitState.Backup:
                    unit.State = PoliceUnitState.TryRelocate;
                    tryingToRelocateUnits.Add(unit);
                    break;

                case PoliceUnitState.TryRelocate:
                    tryingToRelocateUnits.Add(unit);
                    break;

                case PoliceUnitState.Calm:
                    break;

                default:
                    Debug.LogWarning("Unhandled PoliceUnitState in checkForSuspect: " + unit.State + " of " + unit);
                    break;
            }

            if (stateMachine.SuspectLastSeenPosition.HasValue &&
                unitPositionCheckedSuspectPosition(unit.CityPosition, stateMachine.SuspectLastSeenPosition.Value))
            {
                lastSeenPositionReached = true;
            }
        }

        if (tryingToRelocateUnits.Count > 0 && !lastSeenPositionReached)
        {
            // Still trying to relocate around last seen position
            stateMachine.State = PoliceAISystemState.Chase;
        }
        else
        {
            // Gave up: suspect lost, calm down and seek again
            resetLastSeenPosition(stateMachine);
            stateMachine.State = PoliceAISystemState.Seek;

            foreach (var unit in stateMachine.PoliceUnits)
            {
                unit.State = PoliceUnitState.Calm;
            }
        }
    }
    void updateUnits(PoliceAIContext stateMachine)
    {
        foreach(var unit in stateMachine.PoliceUnits)
        {
            switch (unit.State)
            {
                case PoliceUnitState.Chase:
                    updateUnitChase(unit, stateMachine);
                    break;
                case PoliceUnitState.Backup:
                    updateUnitBackup(unit, stateMachine);
                    break;
                case PoliceUnitState.TryRelocate:
                    updateUnitTryRelocate(unit, stateMachine);
                    break;
                case PoliceUnitState.Calm:
                    updateUnitCalm(unit, stateMachine);
                    // Logic for calm behavior
                    break;
                default:
                    Debug.LogWarning("Unhandled PoliceUnitState in updateUnits: " + unit.State + "of " + unit);
                    break;
            }
        }
    }

    bool unitPositionCheckedSuspectPosition(CityPosition unitPosition, CityPosition suspectPosition)
    {
        const float reachedThreshold = 1.0f; // tune this

        var unitPos = unitPosition.WorldPosition;
        var suspectPos = suspectPosition.WorldPosition;

        return Vector2.Distance(unitPos, suspectPos) <= reachedThreshold;
    }

    private void updateUnitCalm(PoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        unit.Movement.TempoState = TempoState.Slow;
        unit.TargetPosition = null;
    }

    private void updateUnitTryRelocate(PoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        unit.Movement.TempoState = TempoState.Medium;
        var edge = stateMachine.SuspectLastSeenPosition?.Edge;
        var node = stateMachine.SuspectLastSeenPosition?.Node;
        var unitPosWorld = unit.CityPosition.WorldPosition;
        if (edge != null)
        {
            var toToPosition = unitPosWorld - edge.To.Position;
            var fromToPosition = unitPosWorld - edge.From.Position;

            unit.TargetPosition = toToPosition.sqrMagnitude < fromToPosition.sqrMagnitude ?
                edge.To.Position :
                edge.From.Position;
        }
        else if (node != null)
        {
            unit.TargetPosition = node.Position;
        }

        
    }

    private void updateUnitBackup(PoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        //for now
        updateUnitCalm(unit, stateMachine);
    }

    private void updateUnitChase(PoliceUnitAIData unit, PoliceAIContext stateMachine)
    {
        unit.Movement.TempoState = TempoState.Fast;
        unit.TargetPosition = stateMachine.SuspectRealPosition.WorldPosition;
    }

    private void updateLastSeenPosition(PoliceAIContext stateMachine, CityPosition suspectPosition)
    {
        // Update the last seen edge based on the suspect's position
        // if suspect is on a node, equivalently set to null
        stateMachine.SuspectLastSeenPosition = suspectPosition;
    }

    private void resetLastSeenPosition(PoliceAIContext stateMachine)
    {
        stateMachine.SuspectLastSeenPosition = null;
    }

    public TempoState OnEdge(PoliceAIContext stateMachine, PoliceUnitAIData unit, RoadEdge edge)
    {
        // Extension point: adjust tempo based on edge characteristics, system state, personality, etc.
        return unit.Movement.TempoState;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateMachine"></param>
    /// <param name="unit"></param>
    /// <param name="turnNode"></param>
    /// <param name="lastEdge"></param>
    /// <returns> preferred edge to go further, null to stay on the node</returns>
    public RoadEdge PickTurn(PoliceAIContext stateMachine, PoliceUnitAIData unit,
        RoadNode turnNode, RoadEdge lastEdge)
    {
        Debug.Assert(stateMachine != null);
        Debug.Assert(unit != null);
        Debug.Assert(turnNode != null);
        //TODO change depending on personality?
        if (unit.TargetPosition != null)
        {
            return pickEdgeCloserToTarget(turnNode, unit.TargetPosition.Value, lastEdge);
        }
        else
        {
            return pickRandomEdge(turnNode, lastEdge);
        }
    }

    private RoadEdge pickRandomEdge(RoadNode turnNode, RoadEdge lastEdge)
    {
        List<RoadEdge> possibleEdges = new List<RoadEdge>();
        foreach (var edge in turnNode.Outgoing)
        {
            if (edge != lastEdge)
            {
                possibleEdges.Add(edge);
            }
        }
        if (possibleEdges.Count == 0)
        {
            // only option is to go back
            return lastEdge;
        }
        int index = Random.Range(0, possibleEdges.Count);
        return possibleEdges[index];
    }

    private RoadEdge pickEdgeCloserToTarget(RoadNode turnNode, Vector2 targetPosition, RoadEdge lastEdge)
    {
        List<(RoadEdge, float)> edgeDistances = new List<(RoadEdge, float)>();
        float lastEdgeDistance = float.MaxValue;
        foreach (var edge in turnNode.Outgoing)
        {
            var otherNode = edge.To == turnNode ? edge.From : edge.To;
            float distance = Vector2.Distance(otherNode.Position, targetPosition);
            if (lastEdge == edge)
            {
                // skipping last edge, since we don't want to go back unless it's the only option
                lastEdgeDistance = distance;
            }
            else
                edgeDistances.Add((edge, distance));
        }
        if (edgeDistances.Count == 0)
        {
            // No outgoing edges found; fallback
            return lastEdge;
        }

        edgeDistances.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        if (lastEdge != null)
        {
            // only add the last edge as the last option
            edgeDistances.Add((lastEdge, lastEdgeDistance));
        }
        // return the edge that gets closer to the suspect
        return edgeDistances[0].Item1;
    }
}


public class PoliceAIContext
{
    public PoliceUnitAIData[] PoliceUnits { get; }
    public CityPosition SuspectRealPosition { get; set; }
    public float CloseInDistance { get; set; } = 20f;
    public CityPosition? SuspectLastSeenPosition { get; set; }
    public PoliceAISystemState State { get; set; }

    public PoliceAIContext(PoliceUnitAIData[] policeUnits)
    {
        PoliceUnits = policeUnits ?? Array.Empty<PoliceUnitAIData>();
        State = PoliceAISystemState.Seek;
    }
}

public enum PoliceAISystemState
{
    Seek,
    Chase
}

public interface PoliceUnitAIData
{
    CityPosition CityPosition { get; }
    Vector2? TargetPosition { get; set; }
    IAIMovement Movement { get; }
    IVision Vision { get; }
    PoliceUnitState State { get; set; }
    PersonalityTag Personality { get; }
}

public interface IAIMovement
{
    bool TowardsTarget { get; set; }
    TempoState TempoState { get; set; }
}

public interface IVision
{
    public bool CanSeeTarget(CityPosition target);
}

// Turning happens instantly so no need to model it??
public enum TempoState
{
    Hold,
    Slow,
    Medium,
    Fast
}

public enum PersonalityTag
{
    Blinky,
    Pinky,
    Inky,
    Clyde
}

public enum PoliceUnitState
{
    /// <summary>
    /// No threat detected, patrolling normally
    /// </summary>
    Calm,
    /// <summary>
    /// Suspect is in sight, actively pursuing
    /// </summary>
    Chase,
    /// <summary>
    /// Last known position of suspect, trying to relocate
    /// </summary>
    TryRelocate,
    /// <summary>
    /// Closing in to the probable area, when too far away
    /// </summary>
    ClosingIn,
    /// <summary>
    /// Suspect is in sight of another unit, moving to provide support
    /// </summary>
    Backup,
    /// <summary>
    /// Avoid the suspect by spreading out
    /// </summary>
    Scatter
}