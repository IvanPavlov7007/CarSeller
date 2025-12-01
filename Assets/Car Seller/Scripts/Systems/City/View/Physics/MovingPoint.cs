using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPoint : MonoBehaviour
{
    
    public static float maxSpeed = 2f;

    City.CityPosition positionData;

    IDirectionProvider directionProvider;
    Transform arrowRotationPoint;
    Transform body;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        body = transform.GetChild(0);
        arrowRotationPoint = transform.GetChild(1);
    }

    public void Initialize(City.CityPosition posData)
    {
        positionData = posData;
    }


    void LateUpdate()
    {

       Vector2 nextDirection = Vector2.ClampMagnitude(directionProvider.ProvidedDirection,1f);

        float lerpV = 10f * Time.deltaTime;
        arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation,  Quaternion.FromToRotation(Vector2.up, nextDirection.normalized),lerpV);
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(nextDirection.magnitude), lerpV), 1f);

        //Anchor node - node from which we move from
        Node a = positionData.NodeA;
        //target node- node to which we move to, can be null if we are just starting from NodeA
        Node b;
        Vector2 dir_a_to_b = Vector2.zero; 
        

        //First check if we exactly on a node
        if (positionData.NodeB == null)
        {
            //Check if there is a neighbour in the desired direction
            b = a.PickClosestNeighbourDirection(nextDirection, out dir_a_to_b);
            //No neighbour in that direction
            if (Vector2.Dot(dir_a_to_b, nextDirection) < 0f || b == null)
            {
                body.up = dir_a_to_b;
                return;
            }
        }
        else
        {
            //keeping moving towards NodeB
            b = positionData.NodeB;
            dir_a_to_b = (b.CurrentPosition - a.CurrentPosition).normalized;
        }

        float t = positionData.RelativePosition;
        //Check if we need to swap nodes
        float dot = Vector2.Dot(nextDirection, dir_a_to_b);
        if (dot < 0)
        {
            Node c = a;
            a = b;
            b = c;
            t = 1f - t;
            dir_a_to_b *= -1f;
        }

        float dist_a_to_b = (b.CurrentPosition - a.CurrentPosition).magnitude;

        Vector2 currentPos = a.CurrentPosition + dir_a_to_b * dist_a_to_b * t;

        //Luftlinie[de] direction we want to go
        Vector2 nextStepInDir = nextDirection * Time.deltaTime * maxSpeed;
        float stepLength = nextStepInDir.magnitude;
        //Luftlinie[de] position of where we would be if there were no nodes
        Vector2 nextIdealPos = currentPos + nextStepInDir;

        //water flow in a noded network
        while (stepLength >= dist_a_to_b * (1f - t))// While step is longer than remaining distance to B
        {
            stepLength -= dist_a_to_b * (1f -t);
            a = b;
            b = b.PickClosestNeighbourDirection(nextDirection, out dir_a_to_b);

            if (Vector2.Dot(dir_a_to_b, nextDirection) < 0f)
            {
                //Cannot proceed further, stop at the current node
                stepLength = 0f;
                b = null;
                break;
            }

            
            dist_a_to_b = (b.CurrentPosition - a.CurrentPosition).magnitude;
            nextDirection = nextIdealPos - a.CurrentPosition;
            t = 0f;
            //if (stepLength > dist_a_to_b)
            //{
            //    t = 0f;
            //}
            //else
            //{
            //    t = stepLength / dist_a_to_b;
            //}

            //wierd code before
            //dist_a_to_b = (nextIdealPos - (Vector2)b.CurrentPosition).magnitude;
        }

        if (b == null)
        {
            //We have reached the end of the line, stop at node A
            transform.position = a.CurrentPosition;
            positionData.SetAtNode(a);
            body.up = dir_a_to_b;
            return;
        }
        else
        {
            t = (dist_a_to_b * t + stepLength) / dist_a_to_b;

            positionData.SetBetween(a, b, t);

            //wierd code before
            //currentPos = CommonTools.GetPerpendicularPointFromPointToLine(nextIdealPos, b.CurrentPosition, a.CurrentPosition);
            //Vector2 curToNewStand = currentPos - curNPos;
        }

        body.up = dir_a_to_b;
        transform.position = positionData.WorldPosition;
    }
}
