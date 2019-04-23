using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackType
{
    //the weapon orientation mode required to perform this attack, if any
    WeaponOrientationMode requiredWeaponType { get; }

    //the boss types the boss must be of to use this attack type, if any
    BossTypeName requiredBossType { get; }

    void ResetAttack();

    void SetupAttack(GameObject performingObj);

    void PerformAttack();
}
