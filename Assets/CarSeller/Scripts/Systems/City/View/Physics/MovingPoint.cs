using Sirenix.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingPoint : MonoBehaviour, IMovement
{
    /// <summary>
    /// Velocity direction provided by the movement system.
    /// </summary>
    public Vector2 ProvidedDirection => currentDirection;
    public float Speed => currentSpeed;
    public float Acceleration { get; private set; }

    ICityPositionable cityPositionable;
    IDirectionProvider directionProvider;
    ISpeedProvider speedProvider;
    Transform arrowRotationPoint;
    Transform body;

    private const float Epsilon = 1e-4f;
    private const float Tolerance = 0.1f; // To avoid floating point issues

    float currentSpeed = 0f;
    Vector2 currentDirection = Vector2.up;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        speedProvider = GetComponent<ISpeedProvider>();
        body = transform.GetChild(0);
        arrowRotationPoint = transform.GetChild(1);
    }

    public void Initialize(ICityPositionable cityPositionable) { this.cityPositionable = cityPositionable; }



    // TODO for cayote: check that all of the returns properly handle it
    void LateUpdate()
    {
        processMovement();
        body.up = currentDirection;
    }

    struct CityPositionWithDistance
    {
        public readonly CityPosition Position;
        public readonly float Distance;
        public CityPositionWithDistance(CityPosition position, float distance)
        {
            Position = position;
            Distance = distance;
        }
    }

    private IReadOnlyList<CityPositionWithDistance> findRecursivePathToClosest(
        RoadEdge currentEdge,
        Vector2 target,
        float minDistance,
        ImmutableHashSet<RoadEdge> visitedEdges)
    {
        if (visitedEdges.Contains(currentEdge))
        {
            return new List<CityPositionWithDistance>();
        }

        CityPosition closest = CityPosition.GetClosestOnEdge(currentEdge, target);
        float closestDistance = Vector2.Distance(closest.WorldPosition, target);
        if (closestDistance >= minDistance)
        {
            return new List<CityPositionWithDistance>();
        }
        minDistance = closestDistance;

        var newVisitedEdges = new HashSet<RoadEdge>(visitedEdges);
        newVisitedEdges.Add(currentEdge);

        var outgoings = new List<RoadEdge>(currentEdge.From.Outgoing);
        outgoings.AddRange(currentEdge.To.Outgoing);

        var currentEntry = new CityPositionWithDistance(closest, closestDistance);

        List<CityPositionWithDistance> bestPath = null;
        foreach (var edge in outgoings)
        {
            if (newVisitedEdges.Contains(edge))
            {
                continue;
            }

            var path = findRecursivePathToClosest(
                    edge,
                    target,
                    closestDistance,
                    new ImmutableHashSet<RoadEdge>(newVisitedEdges))
                .ToList();

            if (path.Count == 0)
            {
                continue;
            }

            var last = path.Last();
            if (last.Distance < minDistance)
            {
                minDistance = last.Distance;
                bestPath = path;
            }
        }

        if (bestPath == null)
        {
            return new List<CityPositionWithDistance>() { currentEntry };
        }

        bestPath.Insert(0, currentEntry);
        return bestPath;
    }

    CityPosition debugFinal;

    private CityPosition processMovementGeneral(CityPosition currentPosition, Vector2 target, float distance)
    {
        List<CityPosition> closestPoints;
        // if we start on a node, we can just pick the best edge and go there
        if (currentPosition.Edge == null)
        {
            closestPoints = bestPathFromNode(currentPosition.Node, target);
            if(closestPoints != null && closestPoints.Count > 0)
            {
                bool nodeIsFrom = closestPoints[0].Edge.From == currentPosition.Node;

                currentPosition = CityPosition.On(closestPoints[0].Edge, nodeIsFrom ? 0f : 1f);
            }
        }
        else
        {
            closestPoints
                = findRecursivePathToClosest(currentPosition.Edge,
                target,
                Mathf.Infinity,
                new ImmutableHashSet<RoadEdge>(new HashSet<RoadEdge>())
                ).Select(p => p.Position).ToList();
        }
        if(closestPoints == null || closestPoints.Count == 0)
        {
            return currentPosition; // No path to target
        }
        debugFinal = closestPoints.Last();
        return moveToNextPoint(closestPoints, currentPosition, distance);
    }

    // Here we assume that closestPoints[0] is on the same edge as current
    private CityPosition moveToNextPoint(IReadOnlyList<CityPosition> closestPoints, CityPosition current, float remainingDistance)
    {
        //how many points
        if (closestPoints.Count == 1)
        {
            return moveToNextPointFinally(current, closestPoints[0], remainingDistance);
        }

        var currentEdgeTargetPosition = current.GetConnectionPositionTowardsEdge(closestPoints[1].Edge);

        // IMPORTANT: connection positions can come back with a different Forward.
        // Align them so all "ahead/behind" and distance computations are consistent.
        if (currentEdgeTargetPosition.Forward != current.Forward)
        {
            currentEdgeTargetPosition = currentEdgeTargetPosition.Reversed();
        }

        RoadEdge currentEdge = current.Edge;
        float stepDistance = current.DistanceToAnotherOnSameEdge(currentEdgeTargetPosition);

        if (remainingDistance > stepDistance)
        {
            var nextCurrent = closestPoints[1].GetConnectionPositionFromAnotherEdge(currentEdge);
            return moveToNextPoint(closestPoints.Skip(1).ToList(), nextCurrent, remainingDistance - stepDistance);
        }

        return moveToNextPointFinally(current, currentEdgeTargetPosition, remainingDistance);
    }

    const float tolerance = 0.01f; // To avoid floating point issues
    private CityPosition moveToNextPointFinally(CityPosition currernt, CityPosition final, float remainingDistance)
    {
        Debug.Assert(currernt.Edge == final.Edge, "Final point must be on the same edge as current point,\n" +
            $"Currently {currernt.Edge} and {final.Edge}");

        // Align parameterization (Forward/Percentage) before any "flow" or distance logic.
        if (final.Forward != currernt.Forward)
        {
            final = final.Reversed();
        }

        if (!currernt.FlowsIntoAnotherOnTheSameEdge(final) && !currernt.Edge.Bidirectional)
        {
            Debug.Log($"Cannot flow from {currernt} to {final} on the same edge");
            // T and forward of both:
            Debug.Log($"Current: {currernt.Percentage} {currernt.Forward}, Final: {final.Percentage} {final.Forward}");
            return currernt; // We cannot move to the final point
        }

        float distanceToFinal = currernt.DistanceToAnotherOnSameEdge(final);
        if (distanceToFinal < tolerance)
        {
            return final; // We are close enough to the final point
        }

        return currernt.TowardsAnotherForDistance(final, Mathf.Min(distanceToFinal, remainingDistance));
    }

    private List<CityPosition> bestPathFromNode(RoadNode node, Vector2 target)
    {
        IReadOnlyList<CityPositionWithDistance> bestOptionPath = null;
        float bestOptionDistance = float.MaxValue;

        foreach (var edge in node.Outgoing)
        {
            var path = findRecursivePathToClosest(
                    edge,
                    target,
                    bestOptionDistance,
                    new ImmutableHashSet<RoadEdge>(new HashSet<RoadEdge>()))
                .ToList();

            if (path.Count == 0)
            {
                continue;
            }

            var last = path.Last();
            if (last.Distance < bestOptionDistance)
            {
                bestOptionDistance = last.Distance;
                bestOptionPath = path;
            }
        }

        if (bestOptionPath == null)
        {
            return null;
        }

        return bestOptionPath.Select(p => p.Position).ToList();
    }

    private void processMovement()
    {
        Vector2 inputDirection = Vector2.ClampMagnitude(directionProvider.ProvidedDirection, 1f);
        Vector2 inputDirectionN = inputDirection.sqrMagnitude > Epsilon ? inputDirection.normalized : Vector2.zero;

        float lerpV = 10f * Time.deltaTime;
        arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation, Quaternion.FromToRotation(Vector2.up, inputDirection.normalized), lerpV);
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(inputDirection.magnitude), lerpV), 1f);

        var cityPosition = cityPositionable.Position;

        Vector2 cappedTarget = inputDirection * 1f + cityPosition.WorldPosition;

        currentSpeed = Mathf.MoveTowards(currentSpeed, speedProvider.Speed, speedProvider.Acceleration * Time.deltaTime);

        Debug.DrawLine(cityPosition.WorldPosition, cappedTarget, Color.green);

        cityPosition = processMovementGeneral(cityPosition, cappedTarget, Time.deltaTime * currentSpeed);

        Debug.Assert(cityPosition.Edge != null || cityPosition.Node != null, "Position must be either on an edge or on a node");

        Vector2 chosenTangentDirection =
            cityPosition.Edge != null ? cityPosition.GetCurrentTangent() :
            (inputDirectionN.sqrMagnitude > Epsilon ? inputDirectionN : currentDirection);

        cityPositionable.Position = cityPosition;
        currentDirection = chosenTangentDirection;
        transform.position = cityPosition.WorldPosition;
    }

    private void processMovementOld()
    {
        Vector2 inputDirection = Vector2.ClampMagnitude(directionProvider.ProvidedDirection, 1f);
        Vector2 inputDirectionN = inputDirection.sqrMagnitude > Epsilon ? inputDirection.normalized : Vector2.zero;

        float lerpV = 10f * Time.deltaTime;
        arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation, Quaternion.FromToRotation(Vector2.up, inputDirection.normalized), lerpV);
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(inputDirection.magnitude), lerpV), 1f);

        var positionData = cityPositionable.Position;

        Vector2 currentPosition = positionData.WorldPosition;

        //Reading data from positionData
        RoadNode a = positionData.Node;//Anchor node - node from which we move from
        RoadEdge edge = positionData.Edge;
        bool forward = positionData.Forward;
        float t = positionData.Percentage;
        Vector2 chosenTangentDirection;


        //First check if we exactly on a node
        if (a != null)
        {
            //Check if there is an edge in the desired direction
            if (TryPickEdgeFromNode(a, inputDirectionN, out edge, out chosenTangentDirection))
            {
                //Check if no neighbor in that direction
                if (Vector2.Dot(chosenTangentDirection, inputDirectionN) < -0.2f)
                {
                    currentSpeed = 0f;
                    // don't change direction if we can't move
                    return;
                }
                //We have an edge to go to
                // calculate forward
                tanDirectionFromNodeAtEdge(a, edge, 0f, out forward);
            }
            else
            {
                currentSpeed = 0f;
                // don't change direction if we can't move
                return; // No edge to go to
            }
        }
        else
        {
            //We are between nodes, need to understand which direction

            if (forward)
            {
                a = edge.From;
                chosenTangentDirection = tanDirectionFromNodeAtEdge(edge.From, edge, t, out _);
            }
            else
            {
                a = edge.To;
                chosenTangentDirection = tanDirectionFromNodeAtEdge(edge.To, edge, t, out _);
            }
        }

        //Check if we need to swap direction on the edge
        float dot = Vector2.Dot(inputDirection, chosenTangentDirection);
        if (dot < -0.2f) // angle more than ~100 degrees
        {
            if (!edge.Bidirectional)
            {
                return; // Cannot go backwards on a non-bidirectional edge
            }
            //Swap direction
            a = (a == edge.From) ? edge.To : edge.From;
            t = 1f - t;
            chosenTangentDirection = -chosenTangentDirection;
            forward = !forward;

            //Reset speed
            currentSpeed = 0f;
        }
        else
        {
            //Accelerate
            currentSpeed = Mathf.MoveTowards(currentSpeed, speedProvider.Speed, speedProvider.Acceleration * Time.deltaTime);
        }

        //Luftlinie[de] direction we want to go
        Vector2 nextStepInDir = inputDirection * Time.deltaTime * currentSpeed;
        float stepLength = nextStepInDir.magnitude;
        //Luftlinie[de] position of where we would be if there were no nodes
        Vector2 nextIdealPos = currentPosition + nextStepInDir;
        float length_a_to_b = edge.Length;


        //water flow in a noded network
        while (stepLength >= length_a_to_b * (1f - t))// While step is longer than remaining distance to B
        {
            stepLength -= length_a_to_b * (1f - t);
            a = (a == edge.From) ? edge.To : edge.From;

            if (TryPickEdgeFromNode(a, inputDirectionN, out edge, out chosenTangentDirection))
            {
                if (Vector2.Dot(chosenTangentDirection, inputDirection) < 0f)
                {
                    //Cannot proceed further, stop at the current node
                    edge = null;
                    stepLength = 0f;
                    break;
                }
            }
            else
            {
                //No edge to proceed further, stop at the current node
                stepLength = 0f;
                break;
            }

            length_a_to_b = edge.Length;
            inputDirectionN = (nextIdealPos - a.Position).normalized;
            t = 0f;
            tanDirectionFromNodeAtEdge(a, edge, 0f, out forward);
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
        cityPositionable.Position = positionData;
        currentDirection = chosenTangentDirection;
        transform.position = positionData.WorldPosition;
    }

    private (RoadEdge, Vector2)[] getOutgoingDirections(RoadNode node)
    {
        (RoadEdge, Vector2)[] neighborsDirection = new (RoadEdge, Vector2)[node.Outgoing.Count];

        for (int i = 0; i < node.Outgoing.Count; i++)
        {
            var edge = node.Outgoing[i];
            neighborsDirection[i] = (edge, edge.GetTangentFromNode(node, 0f, out _));
        }
        return neighborsDirection;
    }

    private Vector2 tanDirectionFromNodeAtEdge(RoadNode node, RoadEdge e, float percentage, out bool forward)
    {
        // Thin wrapper kept for backwards compatibility / readability inside MovingPoint.
        return e.GetTangentFromNode(node, percentage, out forward);
    }

    // Picks the best outgoing edge from a node based on desired direction.
    // Returns false if no suitable edge exists; out leaveDir is what we aim the body at.
    private bool TryPickEdgeFromNode(RoadNode node, Vector2 desired, out RoadEdge edge, out Vector2 leaveDir)
    {
        edge = null;
        leaveDir = Vector2.zero;

        var desiredN = desired.sqrMagnitude > 1e-6f ? desired.normalized : Vector2.zero;
        float bestDot = -Mathf.Infinity;

        foreach (var pair in getOutgoingDirections(node))
        {
            float dot = Vector2.Dot(desiredN, pair.Item2);
            if (dot > bestDot)
            {
                bestDot = dot;
                edge = pair.Item1;
                leaveDir = pair.Item2;
            }
        }
        return edge != null;
    }

    private void OnDrawGizmos()
    {
        Vector2 worldPos = debugFinal.WorldPosition;
        //draw a circle at worldPos
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(worldPos, 0.2f);
    }
}
