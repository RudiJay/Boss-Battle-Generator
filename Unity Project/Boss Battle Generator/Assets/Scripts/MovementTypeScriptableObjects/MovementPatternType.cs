/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMovementPatternType", menuName = "MovementTypeData/MovementPatternType")]
public class MovementPatternType : ScriptableObject
{
    [SerializeField]
    private int numberOfMovements;

    [SerializeField]
    private Vector3[] destinationPoints;
    [SerializeField]
    private bool includeStartPointInDestinations;
    private Vector3 startPoint;
    [SerializeField]
    private bool useXAxis = true, useYAxis = true;
    [SerializeField]
    private bool randomlyDecideNextDestination;
    [SerializeField]
    private float waitTimeAtDestination = 0.0f;

    [SerializeField]
    private VelocityCurveType accelerationType;

    [SerializeField]
    private LateralMovementType lateralMovement;

    //rotation?

    public int GetNumberOfMovements()
    {
        return numberOfMovements;
    }

    public void SetStartPoint(Vector3 value)
    {
        startPoint = value;
    }

    public bool GetIncludeStartPoint()
    {
        return includeStartPointInDestinations;
    }

    public float GetWaitTimeAtDestination()
    {
        return waitTimeAtDestination;
    }

    public VelocityCurveType GetAccelerationType()
    {
        return accelerationType;
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

        if (!useXAxis)
        {
            returnVector.x = startPoint.x;
        }

        if (!useYAxis)
        {
            returnVector.y = startPoint.y;
        }

        return returnVector;
    }
}
