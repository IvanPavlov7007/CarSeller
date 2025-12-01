using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class City : IProductsHolder
{
    public CityConfig Config;
    public Dictionary<object, CityPosition> Objects { get; private set; } = new Dictionary<object, CityPosition>();
    private List<IProductLocation> productLocations = new List<IProductLocation>();
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

    public CityProductLocation GetEmptyProductLocation(CityPosition position)
    {
        return new CityProductLocation(this, position);
    }

    public CityPosition GetClosestPosition(Vector2 worldPosition)
    {
        Node closestNode = nodesNet.GetClosestNode(worldPosition);
        var direction = (worldPosition - closestNode.CurrentPosition).normalized;
        Node closestNeighbour = closestNode.PickClosestNeighbourDirection(direction, out Vector2 closestDirection);
        if(Vector2.Dot(direction, closestDirection) > 0)
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
        relativePos = distanceFromA / totalDistance;
        relativePos = Mathf.Clamp01(relativePos);
        return worldBetweenPos;
    }

    /// <summary>
    /// Use for placing not product objects in the city!
    /// Refactor this later. Problematic to have object as key and there are 2 different ways of placing objects(products and others)
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="position"></param>
    public void PlaceObjectAtPosition(object obj, CityPosition position)
    {
        Objects[obj] = position;
    }

    public CityPosition GetRandomPosition()
    {
        Node random = nodesNet.Nodes[Random.Range(0, nodesNet.Nodes.Count)];
        Node neigbour = random.connectedNeighbors[Random.Range(0, random.connectedNeighbors.Count)];
        float relativePosition = Random.Range(0f, 1f);
        return new CityPosition(random, neigbour, relativePosition);
    }

    public IProductLocation[] GetProductLocations()
    {
        return productLocations.ToArray();
    }

    /// <summary>
    /// Reference to a mutable position in the city, either at a node or between two nodes
    /// </summary>
    public class CityPosition
    {

        public CityPosition(Node nodeA)
        {
            SetAtNode(nodeA);
        }

        public CityPosition(Node nodeA, Node nodeB, float relativePosition)
        {
            SetBetween(nodeA, nodeB, relativePosition);
        }
        public Node NodeA{ get; private set; }
        public Node NodeB { get; private set; }
        public float RelativePosition { get; private set; }

        public void SetAtNode(Node node)
        {
            Debug.Assert(node != null);
            NodeA = node;
            NodeB = null;
            RelativePosition = 0;
        }

        public void SetBetween(Node a, Node b, float t)
        {
            Debug.Assert(a != b);
            Debug.Assert(a.connectedNeighbors.Contains(b));
            Debug.Assert(t >= 0 && t <= 1);

            NodeA = a;
            NodeB = b;
            RelativePosition = t;
        }

        public Vector2 WorldPosition =>
            NodeB == null ? NodeA.CurrentPosition :
            Vector2.Lerp(NodeA.CurrentPosition, NodeB.CurrentPosition, RelativePosition);
    }

    public class CityProductLocation : IProductLocation
    {
        public CityPosition CityPosition { get; private set; }
        public CityProductLocation(City city, CityPosition initialCityPosition, Product product = null)
        {
            Debug.Assert(initialCityPosition != null, "Initial city position cannot be null");

            City = city;
            this.CityPosition = initialCityPosition;
            if(product != null)
                Attach(product);
        }

        public City City { get; private set; }

        public Product Product { get; private set; }

        public IProductsHolder Holder => City;

        public bool Attach(Product product)
        {
            Debug.Assert(product != null, "Product to attach cannot be null");
            if (this.Product == null && product != null)
            {
                Product = product;
                City.productLocations.Add(this);
                City.Objects[product] = CityPosition;
                return true;
            }
            return false;
        }

        public void Detach()
        {
            City.productLocations.Remove(this);
            City.Objects.Remove(Product);
            Product = null;
        }
    }
}