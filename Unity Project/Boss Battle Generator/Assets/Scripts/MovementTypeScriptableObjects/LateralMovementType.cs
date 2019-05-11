/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLateralMovementType", menuName = "MovementTypeData/LateralMovementType")]
public class LateralMovementType : ScriptableObject
{
    public float lateralAmplitude;

    public VelocityCurveType lateralCurveType;
}
