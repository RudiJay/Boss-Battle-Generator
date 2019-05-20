/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface determining the baseline for all boss attack types
/// </summary>
public interface IAttackType
{
    /// <summary>
    /// Time in seconds for attack sequence to wait after performing this attack
    /// </summary>
    float DelayAfterAttack { get; set; }

    //the weapon orientation mode required to perform this attack. if empty, does not use a weapon
    /// <summary>
    /// Returns the weapon orientation modes a boss weapon must be one of to perform this attack. If empty, the attack does not use a weapon.
    /// </summary>
    /// <returns>Enum containing weapon orientation modes a boss weapon must be one of to perform this attack</returns>
    WeaponOrientationMode GetRequiredWeaponTypes();
    
    /// <summary>
    /// Returns the boss types that can use this attack type, if any
    /// </summary>
    /// <returns>Enum containing boss types that can use this attack type</returns>
    BossTypeName GetCompatibleBossTypes();

    /// <summary>
    /// Sets up this attack before performing it
    /// </summary>
    /// <param name="performingObj">The object to perform the attack using, if one is required</param>
    void SetupAttack(GameObject performingObj = null);

    /// <summary>
    /// Performs the attack
    /// </summary>
    void PerformAttack();
}
