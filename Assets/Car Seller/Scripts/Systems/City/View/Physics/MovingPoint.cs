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


        Node currentNode = positionData.NodeA;
        Node nextNode = positionData.NodeB == null ? currentNode.PickClosestNeighbourDirection(nextDirection, out _) : positionData.NodeB;

        Vector2 nextNPos = nextNode.CurrentPosition;
        Vector2 curNPos = currentNode.CurrentPosition;

        Vector2 curToNextVec = (nextNPos - curNPos);

        float dot = Vector2.Dot(nextDirection, curToNextVec);

        if (dot < 0)
        {
            Node c = currentNode;
            currentNode = nextNode;
            nextNode = c;
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
            currentNode = nextNode;
            nextNode = nextNode.PickClosestNeighbourDirection(nextDirection, out _);

            nextNPos = nextNode.CurrentPosition;
            curNPos = currentNode.CurrentPosition;

            curToNextVec = nextNPos - curNPos;

            lastDist = distToNextNode;
            distToNextNode = (nextIdealPos - (Vector2)nextNode.CurrentPosition).magnitude;
        }
        
        newStandingPoint = CommonTools.GetPerpendicularPointFromPointToLine(nextIdealPos, nextNPos, curNPos);
        Vector2 curToNewStand = newStandingPoint - curNPos;

        relativePosBetweenNodes = relativePosBetweenNodes + stepLenght / curToNextVec.magnitude;

        transform.position = curNPos + curToNextVec * relativePosBetweenNodes;
    }
}
