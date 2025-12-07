using System.Collections.Generic;
using UnityEngine;

public class City : ILocationsHolder
{
    public CityConfig Config;
    public Dictionary<ILocatable, CityPosition> Positions { get; private set; } = new Dictionary<ILocatable, CityPosition>();
    private readonly List<ILocation> locations = new List<ILocation>();
    public INodesNetwork nodesNet { get; private set; }

    public City(CityConfig config)
    {
        this.Config = config;
        nodesNet = new SimpleGridNetwork(
            config.cityLeftUpPos,
            config.gridNodesCountX,
            config.gridNodesCountY,
            config.gridNodeWorldSize);
    }

    // Always constructs a new independent location with its own position instance
    public CityLocation GetEmptyLocation(CityPosition position) => new CityLocation(this, position);

    public CityPosition GetClosestPosition(Vector2 worldPosition)
    {
        Node closestNode = nodesNet.GetClosestNode(worldPosition);
        var direction = (worldPosition - closestNode.CurrentPosition).normalized;
        Node closestNeighbour = closestNode.PickClosestNeighbourDirection(direction, out Vector2 closestDirection);
        if (Vector2.Dot(direction, closestDirection) > 0)
        {
            closestPointBetweenNodesPerpendicularToPoint(closestNode, closestNeighbour, worldPosition, out float relativePos);
            return new CityPosition(closestNode, closestNeighbour, relativePos);
        }
        return new CityPosition(closestNode);
    }

    private Vector2 closestPointBetweenNodesPerpendicularToPoint(Node nodeA, Node nodeB, Vector2 worldPoint, out float relativePos)
    {
        Debug.Assert(nodeA != nodeB, "Nodes cannot be the same");
        Debug.Assert(nodeA.connectedNeighbors.Contains(nodeB), "Nodes are not connected");

        Vector2 worldBetweenPos = CommonTools.GetPerpendicularPointFromPointToLine(worldPoint, nodeA.CurrentPosition, nodeB.CurrentPosition);
        float totalDistance = Vector2.Distance(nodeA.CurrentPosition, nodeB.CurrentPosition);
        float distanceFromA = Vector2.Distance(nodeA.CurrentPosition, worldBetweenPos);
        relativePos = Mathf.Clamp01(distanceFromA / totalDistance);
        return worldBetweenPos;
    }

    public void PlaceAtPosition(ILocatable locatable, CityPosition position)
    {
        Debug.Assert(locatable != null, "Locatable cannot be null");
        // Value-type copy ensures no shared mutable state
        Positions[locatable] = position;
    }

    public CityPosition GetRandomPosition()
    {
        Node random = nodesNet.Nodes[Random.Range(0, nodesNet.Nodes.Count)];
        Node neighbour = random.connectedNeighbors[Random.Range(0, random.connectedNeighbors.Count)];
        float relativePosition = Random.Range(0f, 1f);
        return new CityPosition(random, neighbour, relativePosition);
    }

    public ILocation[] GetLocations() => locations.ToArray();

    public readonly struct CityPosition
    {
        public CityPosition(Node nodeA)
        {
            Debug.Assert(nodeA != null);
            NodeA = nodeA;
            NodeB = null;
            RelativePosition = 0f;
        }

        public CityPosition(Node nodeA, Node nodeB, float relativePosition)
        {
            Debug.Assert(nodeA != null);
            Debug.Assert(nodeB != null);
            Debug.Assert(nodeA != nodeB);
            Debug.Assert(nodeA.connectedNeighbors.Contains(nodeB));
            Debug.Assert(relativePosition >= 0f && relativePosition <= 1f);

            NodeA = nodeA;
            NodeB = nodeB;
            RelativePosition = relativePosition;
        }

        public Node NodeA { get; }
        public Node NodeB { get; }
        public float RelativePosition { get; }

        public Vector2 WorldPosition =>
            NodeB == null ? NodeA.CurrentPosition :
            Vector2.Lerp(NodeA.CurrentPosition, NodeB.CurrentPosition, RelativePosition);

        // Helper to create a new position along the same edge
        public CityPosition WithRelative(float t) => new CityPosition(NodeA, NodeB, t);

        // Helper to create a new position at a node
        public static CityPosition At(Node node) => new CityPosition(node);

        // Helper to create a new position between nodes
        public static CityPosition Between(Node a, Node b, float t) => new CityPosition(a, b, t);
    }

    public class CityLocation : ILocation
    {
        public CityPosition CityPosition { get; private set; }
        public City City { get; private set; }
        public ILocatable Occupant { get; private set; }

        public CityLocation(City city, CityPosition initialCityPosition, ILocatable initialOccupant = null)
        {
            City = city;
            CityPosition = initialCityPosition;

            City.locations.Add(this);
            if (initialOccupant != null) Attach(initialOccupant);
        }

        public ILocationsHolder Holder => City;

        public bool Attach(ILocatable locatable)
        {
            Debug.Assert(locatable != null, "Locatable to attach cannot be null");
            if (Occupant == null)
            {
                Occupant = locatable;
                City.Positions[locatable] = CityPosition;
                return true;
            }
            return false;
        }

        public void Detach()
        {
            if (Occupant != null)
            {
                City.Positions.Remove(Occupant);
                Occupant = null;
            }
        }

        // Optional: controlled movement API to prevent external mutation
        public void MoveTo(CityPosition newPosition)
        {
            CityPosition = newPosition;
            if (Occupant != null)
            {
                City.Positions[Occupant] = CityPosition;
            }
        }
    }
}