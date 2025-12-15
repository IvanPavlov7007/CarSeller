using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
    
public class MovingPoint : MonoBehaviour
{
    public static float maxSpeed = 2f;

    City.CityLocation mutableLocation;
    IDirectionProvider directionProvider;
    Transform arrowRotationPoint;
    Transform body;

    private const float Epsilon = 1e-4f;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        body = transform.GetChild(0);
        arrowRotationPoint = transform.GetChild(1);
    }

    public void Initialize(City.CityLocation mutableLocation) { this.mutableLocation = mutableLocation; }



    // TODO for cayote: check that all of the returns properly handle it
    void LateUpdate()
    {
        Vector2 inputDirection = Vector2.ClampMagnitude(directionProvider.ProvidedDirection, 1f);
        Vector2 inputDirectionN = inputDirection.sqrMagnitude > Epsilon ? inputDirection.normalized : Vector2.zero;

        float lerpV = 10f * Time.deltaTime;
        arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation, Quaternion.FromToRotation(Vector2.up, inputDirection.normalized), lerpV);
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(inputDirection.magnitude), lerpV), 1f);

        var positionData = mutableLocation.CityPosition;

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
                //Check if no neighbour in that direction
                if (Vector2.Dot(chosenTangentDirection, inputDirectionN) < 0f)
                {
                    body.up = chosenTangentDirection;
                    return;
                }
                //We have an edge to go to
                // calculate forward
                tanDirectionFromNodeAtEdge(a, edge, 0f, out forward);
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
        }

        //Luftlinie[de] direction we want to go
        Vector2 nextStepInDir = inputDirection * Time.deltaTime * maxSpeed;
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
            positionData = City.CityPosition.At(a);
        }
        else
        {
            t = (t * length_a_to_b + stepLength) / length_a_to_b;

            positionData = City.CityPosition.On(edge, t, forward);
        }
        mutableLocation.SetCityPosition(positionData);
        body.up = chosenTangentDirection;
        transform.position = positionData.WorldPosition;
    }

    private (RoadEdge, Vector2)[] getOutgoingDirections(RoadNode node)
    {
        (RoadEdge, Vector2)[] neighborsDirection = new (RoadEdge, Vector2)[node.Outgoing.Count];

        for (int i = 0; i < node.Outgoing.Count; i++)
        {
            var edge = node.Outgoing[i];
            neighborsDirection[node.Outgoing.IndexOf(edge)] =
                (edge, tanDirectionFromNodeAtEdge(node, edge, 0f, out _));
        }
        return neighborsDirection;
    }

    private Vector2 tanDirectionFromNodeAtEdge(RoadNode node, RoadEdge e, float percentage, out bool forward)
    {
        Debug.Assert(e.From == node || e.To == node, "Edge is not connected to the given node.");
        Debug.Assert(e.GetSpline() != null, "Edge spline is null.");
        var spline = e.GetSpline();
        forward = true;

        // If this node is the 'From', leaving along t = percentage
        if (node == e.From)
        {
            var tanLocal = SplineUtility.EvaluateTangent(spline, percentage);
            var tanWorld3 = e.Container != null
                ? e.Container.transform.TransformDirection((Vector3)tanLocal)
                : (Vector3)tanLocal;
            var outDir = ((Vector2)tanWorld3).normalized;
            return outDir;
        }
        // If this node is the 'To', leaving along -tangent at t = 1 - percentage (requires bidirectional)
        else if (node == e.To && e.Bidirectional)
        {
            forward = false;
            var tanLocal = SplineUtility.EvaluateTangent(spline, 1f - percentage);
            var tanWorld3 = e.Container != null
                ? e.Container.transform.TransformDirection((Vector3)tanLocal)
                : (Vector3)tanLocal;
            var outDir = -((Vector2)tanWorld3).normalized;
            return outDir;
        }
        else
        {
            // Provide detailed context: IDs may be null; include bidirectional state and endpoint relation.
            string nodeId = node != null ? node.Id : "null";
            string edgeId = e != null ? e.Id : "null";
            bool isFrom = e != null && node == e.From;
            bool isTo = e != null && node == e.To;
            bool bidi = e != null && e.Bidirectional;

            throw new System.InvalidOperationException(
                $"Cannot compute tangent direction from node '{nodeId}' on edge '{edgeId}'. " +
                $"Relationship: isFrom={isFrom}, isTo={isTo}, edgeBidirectional={bidi}. " +
                $"If leaving from 'To' endpoint, the edge must be bidirectional."
            );
        }
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
}
