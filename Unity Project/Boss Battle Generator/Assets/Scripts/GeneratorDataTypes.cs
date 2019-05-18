/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using UnityEngine;

/// <summary>
/// Set up enum flags attribute
/// </summary>
public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}

/// <summary>
/// Names of each boss type
/// </summary>
public enum BossTypeName
{
    Random,
    Rocketship,
    FlyingSaucer,
    Starfighter,
    SpaceBattleship,
    AstroMonster
}

/// <summary>
/// Names of each basic shape sprite type
/// </summary>
public enum ShapeTypeName
{
    CIRCLE,
    RING,
    OVAL,
    HALO,
    SEMICIRCLE,
    SEMIOVAL,
    SQUARE,
    RECT,
    DIAMOND,
    RHOMBUS,
    EQUITRI,
    ISOTRI,
    RANGLETRI,
    SCALENETRI,
    PENT,
    IPENT,
    HEX,
    IHEX,
    FIVESTAR,
    SIXSTAR
}

/// <summary>
/// Bitmask enum of each weapon orientation type
/// </summary>
public enum WeaponOrientationMode
{
    FIXEDFORWARD,
    FIXEDSIDEWAYS,
    FIXEDOTHERFORWARDS,
    FIXEDOTHER,
    ROTATABLE,
    TRACKSPLAYER,
    NONORIENTED
}

/// <summary>
/// Serialisable struct containing variables for each boss type
/// </summary>
[System.Serializable]
public struct BossType
{
    public BossTypeName typeName;
    [Space(10)]
    public AnimationCurve spriteComplexityCurve;
    [Space(10)]
    public AnimationCurve weaponQuantityCurve;
    [Space(10)]
    [Range(0, 1)]
    public float[] shapeProbability;

    [Header("Symmetry Multipliers")]
    [Space(5)]
    public float asymmetricProbabilityMultiplier;
    public float normaliseRotProbabilityMultiplier;
    public float centreXProbabilityMultiplier;
    public float mirrorProbabilityMultiplier;

}

/// <summary>
/// Serialisable struct containing variables for each basic shape sprite
/// </summary>
[System.Serializable]
public struct ShapeType
{
    public ShapeTypeName shapeName;

    public Sprite sprite;

    public bool twoDimensionSizeGeneration;

    public bool generateRotation;

    public float nearestSymmetricalRot;

    [Header("Symmetry")]
    [Space(5)]
    public float AsymmetricProbability;
    public float NormaliseRotProbability;
    public float CentreXAndNormaliseRotProbability;
    public float MirrorProbability;
}

/// <summary>
/// Serialisable struct containing variables for each weapon type
/// </summary>
[System.Serializable]
public struct WeaponType
{
    public Sprite sprite;

    public float size;

    public Vector3 spritePivotPointOffset;
    public Vector3 attackSourcePositionOffset;

    [EnumFlags]
    public WeaponOrientationMode availableWeaponOrientations;

    [EnumFlags]
    public BossTypeName bossTypesWeaponWieldableBy;

    public bool generateWeaponColor;

    public bool canWeaponFloat;

    [Header("Symmetry")]
    [Space(5)]
    public float AsymmetricProbability;
    public float CentreXProbability;
    public float MirrorProbability;
}

/// <summary>
/// Serialisable struct containing variables determining projectile behaviour
/// </summary>
[System.Serializable]
public struct Projectile
{
    public Vector2 scale;
    public Sprite projectileSprite;
    public Color colorOverlay;

    public float travelSpeed;

    public float rotationSpeed;

    public bool tracksPlayer;
    public float trackingTime;
    public float trackingStartupTime;

    public float selfDestructTime;
}
