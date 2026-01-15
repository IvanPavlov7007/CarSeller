using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using CityPosition = City.CityPosition;
using Random = UnityEngine.Random;
public class PoliceAISystem
{
    public void UpdateState(PoliceAIStateMachine state)
    {
        checkForSuspect(state);
        updateUnits(state);
    }

    void checkForSuspect(PoliceAIStateMachine stateMachine)
    {
        List<PoliceUnitAIData> unitsSeeingSuspect = new List<PoliceUnitAIData>();
        List<PoliceUnitAIData> tryingToRelocateUnits = new List<PoliceUnitAIData>();

        foreach (var unit in stateMachine.policeUnits)
        {
            if (unit.Vision.CanSeeTarget(stateMachine.SuspectRealPosition))
            {
                unitsSeeingSuspect.Add(unit);
            }
        }
        // If any unit can see the suspect, transition to Chase state
        if (unitsSeeingSuspect.Count > 0)
        {
            foreach (var unit in stateMachine.policeUnits)
            {
                if (unitsSeeingSuspect.Contains(unit))
                {
                    unit.State = PoliceUnitState.Chase;
                }
                else
                {
                    unit.State = PoliceUnitState.Backup;
                }
            }
            updateLastSeenPosition(stateMachine, stateMachine.SuspectRealPosition);
            stateMachine.State = PoliceAISystemState.Chase;
        }
        else
        {
            foreach (var unit in stateMachine.policeUnits)
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
                        Debug.LogWarning("Unhandled PoliceUnitState in checkForSuspect: " + unit.State + "of " + unit);
                        break;
                }
                if (stateMachine.State == PoliceAISystemState.Chase &&
                    unitPositionCheckedSuspectPosition(unit.CityPosition, stateMachine.SuspectLastSeenPosition.Value))
                {
                    // if unit has reached last seen position and no suspect has been spotted, go calm
                    tryingToRelocateUnits.Clear();
                    break;
                }
            }

            if (tryingToRelocateUnits.Count > 0)
            {
                stateMachine.State = PoliceAISystemState.Chase;
            }
            else
            {
                resetLastSeenPosition(stateMachine);
                stateMachine.State = PoliceAISystemState.Seek;
                foreach (var unit in stateMachine.policeUnits)
                {
                    unit.State = PoliceUnitState.Calm;
                }
            }
        }
    }
    void updateUnits(PoliceAIStateMachine stateMachine)
    {
        foreach(var unit in stateMachine.policeUnits)
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
        if(suspectPosition.Edge != null)
        {
            return unitPosition.Edge == suspectPosition.Edge;
        }
        else if(suspectPosition.Node != null)
        {
            // suspect position is on a node
            return unitPosition.Node == suspectPosition.Node;
        }
        Debug.LogWarning("Suspect position is neither on an edge nor a node.");
        return false;
    }

    private void updateUnitCalm(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
    {
        unit.Movement.TempoState = TempoState.Slow;
        unit.TargetPosition = null;
    }

    private void updateUnitTryRelocate(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
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

    private void updateUnitBackup(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
    {
        //for now
        updateUnitCalm(unit, stateMachine);
    }

    private void updateUnitChase(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
    {
        unit.Movement.TempoState = TempoState.Fast;
        unit.TargetPosition = stateMachine.SuspectRealPosition.WorldPosition;
    }

    private void updateLastSeenPosition(PoliceAIStateMachine stateMachine, CityPosition suspectPosition)
    {
        // Update the last seen edge based on the suspect's position
        // if suspect is on a node, equivalently set to null
        stateMachine.SuspectLastSeenPosition = suspectPosition;
    }

    private void resetLastSeenPosition(PoliceAIStateMachine stateMachine)
    {
        stateMachine.SuspectLastSeenPosition = null;
    }

    public TempoState OnEdge(PoliceAIStateMachine stateMachine, PoliceUnitAIData unit, RoadEdge edge)
    {
        // for now, just return current tempo
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
    public RoadEdge PickTurn(PoliceAIStateMachine stateMachine, PoliceUnitAIData unit,
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


public class PoliceAIStateMachine
{
    public PoliceUnitAIData[] policeUnits = new PoliceUnitAIData[0];
    public CityPosition SuspectRealPosition;
    public float CloseInDistance = 20f;
    public CityPosition? SuspectLastSeenPosition { get; set; } // for suspect state
    public PoliceAISystemState State;

    public PoliceAIStateMachine(PoliceUnitAIData[] policeUnits)
    {
        this.policeUnits = policeUnits;
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