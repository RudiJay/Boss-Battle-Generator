/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMovementPatternType", menuName = "MovementTypeData/MovementPatternType")]
public class MovementPatternType : ScriptableObject
{
    public int numberOfMovements;

    [SerializeField]
    private Vector3[] destinationPoints;
    public bool includeStartPointInDestinations;
    private Vector3 startPoint;
    [SerializeField]
    private bool constrainXAxis = false, constrainYAxis = false;
    [SerializeField]
    private bool randomlyDecideNextDestination;
    public float waitTimeAtDestination = 0.0f;

    public VelocityCurveType accelerationType;

    [SerializeField]
    private LateralMovementType lateralMovement;

    //rotation?

    public void SetDestinationPoints(Vector3[] inPoints)
    {
        destinationPoints = inPoints;
    }

    public void SetAxisConstraints(bool xAxis, bool yAxis)
    {
        constrainXAxis = xAxis;
        constrainYAxis = yAxis;
    }

    public void SetRandomlyDecideNextDestination(bool value)
    {
        randomlyDecideNextDestination = value;
    }

    public void SetStartPoint(Vector3 value)
    {
        startPoint = value;
    }

    public Vector3 GetNextDestinationPoint(int nextDestinationIndex)
    {
        Vector3 returnVector;

        int arraySize = includeStartPointInDestinations ? destinationPoints.Length + 1 : destinationPoints.Length;

        if (!randomlyDecideNextDestination)
        {
            while (nextDestinationIndex >= arraySize)
            {
                nextDestinationIndex -= arraySize;
            }
        }
        else
        {
            nextDestinationIndex = Random.Range(0, arraySize);
        }

        if (includeStartPointInDestinations && nextDestinationIndex == arraySize - 1)
        {
            returnVector = startPoint;
        }
        else
        {
            returnVector = destinationPoints[nextDestinationIndex];
        }

        if (constrainXAxis)
        {
            returnVector.x = startPoint.x;
        }

        if (constrainYAxis)
        {
            returnVector.y = startPoint.y;
        }

        return returnVector;
    }
}
