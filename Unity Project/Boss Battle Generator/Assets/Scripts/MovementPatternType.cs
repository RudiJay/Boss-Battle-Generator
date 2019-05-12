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
    private bool useXAxis, useYAxis;
    [SerializeField]
    private bool randomlyDecideNextDestination;

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

    public Vector3 GetNextDestinationPoint(int nextDestinationIndex)
    {
        int arraySize = includeStartPointInDestinations ? destinationPoints.Length + 1 : destinationPoints.Length;
        
        while (nextDestinationIndex >= arraySize)
        {
            nextDestinationIndex -= arraySize;
        }

        if (includeStartPointInDestinations && nextDestinationIndex == arraySize - 1)
        {
            return startPoint;
        }

        return destinationPoints[nextDestinationIndex];
    }
}
