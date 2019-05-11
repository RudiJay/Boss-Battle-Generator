/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVelocityCurveType", menuName = "MovementTypeData/VelocityCurveType")]
public class VelocityCurveType : ScriptableObject
{
    [SerializeField]
    private AnimationCurve velocityCurve;

    public AnimationCurve GetCurve()
    {
        return velocityCurve;
    }
}
