/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackType
{
    //the weapon orientation mode required to perform this attack. if empty, does not use a weapon
    WeaponOrientationMode GetRequiredWeaponTypes();

    //the boss types the boss must be of to use this attack type, if any
    BossTypeName GetCompatibleBossTypes();

    void ResetAttack();

    void SetupAttack(GameObject performingObj);

    void PerformAttack();
}
