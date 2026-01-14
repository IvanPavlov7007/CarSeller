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
        List<PoliceUnitAIData> suspectingUnits = new List<PoliceUnitAIData>();

        foreach (var unit in stateMachine.policeUnits)
        {
            if (unit.Vision.CanSeeTarget(stateMachine.suspectRealPosition))
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
            updateLastSeenEdge(stateMachine, stateMachine.suspectRealPosition);
            stateMachine.State = PoliceAISystemState.Chase;
        }
        else
        {
            foreach (var unit in stateMachine.policeUnits)
            {
                if (unit.State == PoliceUnitState.Chase && stateMachine.SuspectLastSeenEdge != null)
                {
                    unit.State = PoliceUnitState.Suspect;
                    suspectingUnits.Add(unit);
                }
                else
                {
                    unit.State = PoliceUnitState.Calm;
                }

                // if the unit is at the last seen edge, clear it
                if (unit.CityPosition.Edge == stateMachine.SuspectLastSeenEdge)
                {
                    stateMachine.SuspectLastSeenEdge = null;
                }
            }

            if (suspectingUnits.Count >= 0)
            {
                stateMachine.State = PoliceAISystemState.Chase;
            }
            else
            {
                stateMachine.State = PoliceAISystemState.Seek;
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
                case PoliceUnitState.Suspect:
                    updateUnitSuspect(unit, stateMachine);
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

    private void updateUnitCalm(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
    {
        unit.Movement.TempoState = TempoState.Slow;
        unit.TargetPosition = unit.CityPosition.WorldPosition;
    }

    private void updateUnitSuspect(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
    {
        var toToPosition = unit.CityPosition.WorldPosition - stateMachine.SuspectLastSeenEdge.To.Position;
        var fromToPosition = unit.CityPosition.WorldPosition - stateMachine.SuspectLastSeenEdge.From.Position;

        unit.TargetPosition = toToPosition.sqrMagnitude < fromToPosition.sqrMagnitude ?
            stateMachine.SuspectLastSeenEdge.To.Position :
            stateMachine.SuspectLastSeenEdge.From.Position;
    }

    private void updateUnitBackup(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
    {
        //for now
        updateUnitCalm(unit, stateMachine);
    }

    private void updateUnitChase(PoliceUnitAIData unit, PoliceAIStateMachine stateMachine)
    {
        unit.Movement.TempoState = TempoState.Fast;
        unit.TargetPosition = stateMachine.suspectRealPosition.WorldPosition;
    }

    private void updateLastSeenEdge(PoliceAIStateMachine stateMachine, CityPosition suspectPosition)
    {
        // Update the last seen edge based on the suspect's position
        // if suspect is on a node, equivalently set to null
        stateMachine.SuspectLastSeenEdge = suspectPosition.Edge;
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
        RoadNode turnNode, RoadEdge lastEdge, RoadEdge suspectLastEdge)
    {
        Debug.Assert(stateMachine != null);
        Debug.Assert(unit != null);
        Debug.Assert(turnNode != null);

        // if the suspect last edge is outgoing from the turn node, go that way 
        if (turnNode.Outgoing.Contains(suspectLastEdge))
        {
            return suspectLastEdge;
        }

        if(stateMachine.suspectRealPosition.Edge == null)
        {
            // suspect is on a node, just pick any edge
            return turnNode.Outgoing[Random.Range(0, turnNode.Outgoing.Count)];
        }

        List<(RoadEdge, float)> edgeDistances = new List<(RoadEdge, float)>();
        float lastEdgeDistance = float.MaxValue;
        foreach (var edge in turnNode.Outgoing)
        {
            var otherNode = edge.To == turnNode ? edge.From : edge.To;
            float distance = Vector2.Distance(otherNode.Position, stateMachine.suspectRealPosition.WorldPosition);
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
    public CityPosition suspectRealPosition;
    public float CloseInDistance = 20f;
    public RoadEdge SuspectLastSeenEdge { get; set; } // for suspect state
    public PoliceAISystemState State;

    public PoliceAIStateMachine(PoliceUnitAIData[] policeUnits)
    {
        this.policeUnits = policeUnits;
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
    Vector2 TargetPosition { get; set; }
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
    Suspect,
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