using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class City : IProductsHolder
{
    public Dictionary<object, CityPosition> Objects { get; private set; } = new Dictionary<object, CityPosition>();
    private List<IProductLocation> productLocations = new List<IProductLocation>();
    public INodesNet nodesNet { get; private set; } = new SimpleGridNet(Vector2.zero, 6, 6, 2f);

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

    public class CityPosition
    {

        public CityPosition(Node nodeA)
        {
            NodeA = nodeA;
        }

        public CityPosition(Node nodeA, Node nodeB, float relativePosition)
        {
            NodeA = nodeA;
            NodeB = nodeB;
            this.RelativePosition = relativePosition;
        }

        Node nodeA;
        Node nodeB;
        float relativePosition;
        public Node NodeA
        {
            get { return nodeA; }
            set
            {
                Debug.Assert(value != null, "NodeA cannot be null");
                NodeA = value;
            }
        }

        public Node NodeB
        {
            get { return nodeB; }
            set
            {
                Debug.Assert(nodeA != nodeB, "Nodes cannot be the same");
                Debug.Assert(nodeA.connectedNeighbors.Contains(nodeB), "Nodes are not connected");
                nodeB = value;
            }
        }
        public float RelativePosition
        {
            get { return relativePosition; }
            set
            {
                Debug.Assert(value >= 0f && value <= 1f, "Relative position must be between 0 and 1");
                relativePosition = value;
            }
        }

        public Vector2 GetWorldPosition()
        {
            if (NodeB == null)
            {
                return NodeA.CurrentPosition;
            }
            else
            {
                return Vector2.Lerp(NodeA.CurrentPosition, NodeB.CurrentPosition, RelativePosition);
            }
        }
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
            if (product == null)
            {
                Product = product;
                City.productLocations.Add(this);
                City.Objects[product] = CityPosition;
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