using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPoint : MonoBehaviour
{
    
    public static float maxSpeed = 2f;

    City.CityPosition positionData;

    IDirectionProvider directionProvider;
    Transform arrowRotationPoint;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        arrowRotationPoint = transform.GetChild(0);
    }

    public void Initialize(City.CityPosition posData)
    {
        positionData = posData;
    }


    void LateUpdate()
    {

       Vector2 nextDirection = directionProvider.ProvidedDirection;

        float lerpV = 10f * Time.deltaTime;
        arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation,  Quaternion.FromToRotation(Vector2.up, nextDirection.normalized),lerpV);
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, nextDirection.magnitude, lerpV), 1f);

        //Anchor node - node from which we move from
        Node a = positionData.NodeA;
        //target node- node to which we move to, can be null if we are just starting from NodeA
        Node b;
        Vector2 a_to_b = Vector2.zero; 

        //First check if we exactly on a node
        if (positionData.NodeB == null)
        {
            //Check if there is a neighbour in the desired direction
            b = a.PickClosestNeighbourDirection(nextDirection, out a_to_b);
            //No neighbour in that direction
            if (Vector2.Dot(a_to_b, nextDirection) < 0f)
            {
                return;
            }
        }
        else
        {
            //keeping moving towards NodeB
            b = positionData.NodeB;
            a_to_b = (b.CurrentPosition - a.CurrentPosition).normalized;
        }

        //TODO old code below, refactor!

        Vector2 nextNPos = b.CurrentPosition;
        Vector2 curNPos = a.CurrentPosition;

        Vector2 curToNextVec = (nextNPos - curNPos);

        //Check if we need to swap nodes
        float dot = Vector2.Dot(nextDirection, curToNextVec);
        if (dot < 0)
        {
            Node c = a;
            a = b;
            b = c;
            relativePosBetweenNodes = 1f - relativePosBetweenNodes;
            curToNextVec *= -1f;
            Vector2 vC = curNPos;
            curNPos = nextNPos;
            nextNPos = curNPos;
        }

        Vector2 newStandingPoint = curNPos + curToNextVec * relativePosBetweenNodes;

        Vector2 nextStepInDir = nextDirection * Time.deltaTime * maxSpeed;
        float stepLenght = nextStepInDir.magnitude;
        Vector2 nextIdealPos = newStandingPoint + nextStepInDir;

        float distToNextNode, lastDist;

        distToNextNode = curToNextVec.magnitude;
        lastDist = (newStandingPoint - curNPos).magnitude;

        while(stepLenght >= distToNextNode *(1f - relativePosBetweenNodes))
        {
            stepLenght -= distToNextNode * (1f -relativePosBetweenNodes);
            relativePosBetweenNodes = 0f;
            a = b;
            b = b.PickClosestNeighbourDirection(nextDirection, out _);

            nextNPos = b.CurrentPosition;
            curNPos = a.CurrentPosition;

            curToNextVec = nextNPos - curNPos;

            lastDist = distToNextNode;
            distToNextNode = (nextIdealPos - (Vector2)b.CurrentPosition).magnitude;
        }
        
        newStandingPoint = CommonTools.GetPerpendicularPointFromPointToLine(nextIdealPos, nextNPos, curNPos);
        Vector2 curToNewStand = newStandingPoint - curNPos;

        relativePosBetweenNodes = relativePosBetweenNodes + stepLenght / curToNextVec.magnitude;

        transform.position = curNPos + curToNextVec * relativePosBetweenNodes;
    }
}
