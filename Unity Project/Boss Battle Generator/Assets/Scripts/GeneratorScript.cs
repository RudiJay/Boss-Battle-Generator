using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Set up enum flags attribute
/// </summary>
public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}

/// <summary>
/// Set up enum flags attribute content
/// </summary>
[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
    }
}

/// <summary>
/// Names of each boss type
/// </summary>
public enum BossTypeName
{
    RANDOM,
    ROCKETSHIP,
    FLYINGSAUCER,
    STARFIGHTER,
    SPACEBATTLESHIP,
    ASTROMONSTER,
    COUNT
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
    SIXSTAR,
    COUNT
}

/// <summary>
/// Bitmask enum of each weapon orientation type
/// </summary>
[System.Flags]
public enum WeaponOrientationMode
{
    FIXEDFORWARD = 1 << 0,
    FIXEDSIDEWAYS = 1 << 1,
    FIXEDOTHER = 1 << 2,
    ROTATABLE = 1 << 3,
    NONORIENTED = 1 << 4
}

/// <summary>
/// Serialisable struct containing variables for each boss type
/// </summary>
[System.Serializable]
public struct BossType
{
    public BossTypeName typeName;

    public AnimationCurve complexityProbability;

    public AnimationCurve symmetryProbability;

    [Range(0,1)]
    public float[] shapeProbability;
}

/// <summary>
/// Serialisable struct containing variables for each basic shape sprite
/// </summary>
[System.Serializable]
public struct ShapeType
{
    public ShapeTypeName shapeName;

    public Sprite sprite;

    [Range(0,1)]
    public float[] symmetryProbBounds;

    public float nearestSymmetricalRot;
}

/// <summary>
/// Serialisable struct containing variables for each weapon type
/// </summary>
[System.Serializable]
public struct WeaponType
{
    public Sprite sprite;

    [EnumFlags]
    public WeaponOrientationMode availableWeaponOrientations;

    [EnumFlags]
    public BossTypeName bossTypesWeaponWieldableBy;

    [Range(0, 1)]
    public float[] symmetryProbBounds;

    public bool canWeaponFloat;
}

public class GeneratorScript : MonoBehaviour
{
    public static GeneratorScript Instance;

    private System.Random rand;

    private GameObject bossObj;
    private SpriteRenderer bossSprite;

    private Camera spriteSnapshotCam;

    private GameObject snapshotSpriteObj;

    private IEnumerator autoGenerator;
    private bool isAutoGenerating = false;

    [Header("Interface")]
    [SerializeField]
    private float autoGeneratorDelay = 2.0f;
    [SerializeField][Tooltip("CAUTION: do not use unless testing sprite creation")]
    private bool DebugShowSpriteBorder = false;

    
    [Header("Generation Variables")]
    [SerializeField][Space(10)]
    private int seed;
    private int symmetricSeed;

    [SerializeField]
    private int maxBossWidth = 500, maxBossHeight = 500;
    private int textureWidth, textureHeight;
    private int xOffset, yOffset;
    [SerializeField][Range(0,1)]
    private float shapeSizeLimiter = 0.75f; 
    [SerializeField]
    private float weaponXLimit = 3, weaponYLimit = 3;

    [Header("Randomisation Scales")]
    [SerializeField][Space(10)]
    private int bossTypeMax = 100;
    [SerializeField]
    private int symmetricMax = 100, shapeMax = 100, weaponTypeMax = 100;

    [Header("Boss Type")]
    [SerializeField][Space(10)]
    private BossType[] BossTypeVariables;
    private BossType bossType;

    [Header("Sprite Shapes")]
    [SerializeField][Space(10)]
    private int spriteShapeComplexity = 3;
    [SerializeField]
    private ShapeType[] SpriteGenerationShapes;

    [Header("Weapons")]
    [SerializeField][Space(10)]
    private int numberOfWeapons = 2;
    [SerializeField]
    private GameObject WeaponPrefab;
    [SerializeField]
    private LayerMask bossSpriteLayer;
    [SerializeField]
    private int maxWeaponTypeAttempts = 10, maxWeaponPosAttempts = 15;
    [SerializeField]
    private WeaponType[] GeneratableWeapons;

    private void Awake()
    {
        //initialise singleton pattern
        Instance = this;
    }

    private void Start()
    {
        //initialise references
        rand = new System.Random();

        autoGenerator = AutoGenerateBossFights();

        bossObj = GameObject.FindWithTag("Boss");
        if (bossObj)
        {
            bossSprite = bossObj.GetComponentInChildren<SpriteRenderer>();
        }

        spriteSnapshotCam = GameObject.FindWithTag("SpriteSnapshotCam").GetComponent<Camera>();

        snapshotSpriteObj = GameObject.FindWithTag("SnapshotSpriteObj");

        //calculate determinant sizes
        textureWidth = (int)(maxBossWidth * 1.25f);
        textureHeight = (int)(maxBossHeight * 1.25f);
        xOffset = (textureWidth - maxBossWidth) / 2;
        yOffset = (textureHeight - maxBossHeight) / 2;
    }

    private void Update()
    {
        //Toggle auto generate input
        if (Input.GetButtonDown("ToggleAutoGenerate"))
        {
            if (!isAutoGenerating)
            {
                StartCoroutine(autoGenerator);
            }
            else
            {
                StopCoroutine(autoGenerator);
            }

            isAutoGenerating = !isAutoGenerating;
        }
    }

    /// <summary>
    /// Generates a new boss repeatedly with a delay
    /// </summary>
    private IEnumerator AutoGenerateBossFights()
    {
        while (true)
        {
            WaitForSeconds delay = new WaitForSeconds(autoGeneratorDelay);

            GenerateBossFight(true);

            yield return delay;
        }
    }

    /// <summary>
    /// Takes a snapshot of a camera in the scene pointed at a shape sprite and draws it onto the input texture
    /// </summary>
    /// <param name="texture">the texture to snapshot shape draw onto</param>
    /// <param name="x0"></param>
    /// <param name="y0"></param>
    private void DrawShapeFromSnapshot(Texture2D texture, int x0, int y0)
    {
        spriteSnapshotCam.targetTexture = RenderTexture.GetTemporary(textureHeight, textureWidth, 16);

        spriteSnapshotCam.Render();

        RenderTexture.active = spriteSnapshotCam.targetTexture;
        float snapWidth = spriteSnapshotCam.targetTexture.width;
        float snapHeight = spriteSnapshotCam.targetTexture.height;

        Texture2D snapshot = new Texture2D(textureWidth, textureHeight);
        snapshot.ReadPixels(new Rect(0, 0, snapWidth, snapHeight), 0, 0);

        RenderTexture.ReleaseTemporary(spriteSnapshotCam.targetTexture);

        x0 -= snapshot.width / 2;
        y0 -= snapshot.height / 2;

        TextureDraw.CopyFromTexture(texture, snapshot, x0, y0);
    }

    /// <summary>
    /// Generates a new seed for the random boss fight generation
    /// </summary>
    public void GenerateSeed()
    {
        seed = System.Environment.TickCount;

        GeneratorUI.Instance.UpdateSeedUI(seed.ToString());

        rand = new System.Random(seed);
    }

    /// <summary>
    /// Sets the seed for the random boss fight generation
    /// </summary>
    /// <param name="inSeed">the number to set the seed to</param>
    public void SetSeed(int inSeed)
    {
        if (inSeed != seed)
        {
            seed = inSeed;

            rand = new System.Random(seed);

            GenerateBossFight(false);
        }
    }

    /// <summary>
    /// Generate a new boss fight
    /// </summary>
    /// <param name="generateNewSeed">whether the new boss needs to generate a new seed or not</param>
    public void GenerateBossFight(bool generateNewSeed)
    {
        if (bossSprite)
        {
            GeneratorUI.Instance.ToggleGeneratingInProgressLabel(true);

            if (generateNewSeed)
            {
                GenerateSeed();
            }

            SetBossType();

            GenerateSymmetrySeed();

            GenerateSprite();

            GenerateColliders();

            GenerateWeapons();

            GeneratorUI.Instance.ToggleGeneratingInProgressLabel(false);
        }
    }

    private void SetBossType()
    {
        BossTypeName typeName;
        typeName = GeneratorUI.Instance.GetBossTypeName();

        if (typeName == BossTypeName.RANDOM)
        {
            int typeSeed = rand.Next(0, bossTypeMax);
            int index = (int)(typeSeed / (float)bossTypeMax * ((int)BossTypeName.COUNT - 1)) + 1; //+1 to avoid setting boss type to RANDOM again
            typeName = (BossTypeName)index;
            Debug.Log("Random Boss Type: " + typeName);
        }

        bossType = System.Array.Find<BossType>(BossTypeVariables, BossTypeVariables => BossTypeVariables.typeName == typeName);
    }

    private void GenerateSymmetrySeed()
    {
        symmetricSeed = rand.Next(0, symmetricMax);
        Debug.Log("Symmetric seed (0, " + symmetricMax + "): " + symmetricSeed);
    }

    /// <summary>
    /// Generates the sprite that makes up the body of the boss
    /// </summary>
    private void GenerateSprite()
    {
        if (spriteSnapshotCam && snapshotSpriteObj && SpriteGenerationShapes.Length > 0)
        {
            SpriteRenderer snapshotSprite = snapshotSpriteObj.GetComponentInChildren<SpriteRenderer>();

            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            //whether to clear out a the space the sprite can occupy, leaving the border around it
            //or just clear out the whole background
            //CAUTION: this border is included in polygon collider generated later on, so do not use unless debugging sprite creation
            if (DebugShowSpriteBorder)
            {
                for (int y = yOffset; y < maxBossHeight + yOffset; y++)
                {
                    for (int x = xOffset; x < maxBossWidth + xOffset; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            else
            {
                TextureDraw.Clear(texture);
            }

            for (int i = 0; i < spriteShapeComplexity; i++)
            {
                int shapeSeed = rand.Next(0, shapeMax);                
                Debug.Log("Shape seed (0, " + shapeMax + "): " + shapeSeed);
                
                snapshotSpriteObj.transform.rotation = Quaternion.identity;

                ShapeType spriteShape;
                int index = (int)(shapeSeed / (float)shapeMax * SpriteGenerationShapes.Length);
                spriteShape = SpriteGenerationShapes[index];

                snapshotSprite.sprite = spriteShape.sprite;

                int x0 = (textureWidth / 2);
                int y0 = (textureHeight / 2);

                switch (spriteShape.shapeName)
                {
                    //radius, no rotation
                    case ShapeTypeName.CIRCLE:
                    case ShapeTypeName.RING:
                        {
                            int radiusMax = maxBossWidth / 2;
                            int radiusMin = radiusMax / 10;
                            int radiusSeed = rand.Next(radiusMin, radiusMax);
                            float scale = radiusSeed / (float)radiusMax;

                            int xMax = maxBossWidth + xOffset - radiusSeed;
                            int xMin = xOffset + radiusSeed;
                            int xSeed = rand.Next(xMin, xMax);

                            int yMax = maxBossHeight + yOffset - radiusSeed;
                            int yMin = yOffset + radiusSeed;
                            int ySeed = rand.Next(yMin, yMax);

                            snapshotSpriteObj.transform.localScale = new Vector3(scale, scale);

                            if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[0])
                            {
                                //put shape in the middle
                                xSeed = textureWidth / 2;
                            }
                            else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[1])
                            {
                                //double up shape on both sides
                                //get opposite xSeed
                                int x2 = textureWidth - xSeed;
                                DrawShapeFromSnapshot(texture, x2, ySeed);
                            }
                            //else no symmetry

                            x0 = xSeed;
                            y0 = ySeed;

                            /*
                            Debug.Log("Radius seed (" + radiusMin + ", " + radiusMax + "): " + radiusSeed + " (" + scale + "%)");
                            Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                            Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            */
                        };
                        break;
                    //single dimension size
                    case ShapeTypeName.SQUARE:
                    case ShapeTypeName.SEMICIRCLE:
                    case ShapeTypeName.EQUITRI:
                    case ShapeTypeName.PENT:
                    case ShapeTypeName.HEX:
                    case ShapeTypeName.FIVESTAR:
                    case ShapeTypeName.SIXSTAR:
                        {
                            int minSize = maxBossWidth / 10;

                            int rotSeed = rand.Next(-180, 180);

                            float theta = rotSeed * Mathf.Deg2Rad;

                            int sizeSeed = rand.Next(minSize, (int)(maxBossWidth * shapeSizeLimiter));
                            float size = sizeSeed / (float)maxBossWidth;

                            int rotSize = (int)(Mathf.Abs(sizeSeed * Mathf.Sin(theta)) + Mathf.Abs(sizeSeed * Mathf.Cos(theta)));

                            int xMax = maxBossWidth + xOffset - (rotSize / 2);
                            int xMin = xOffset + (rotSize / 2);
                            int xSeed = rand.Next(xMin, xMax);

                            int yMax = maxBossHeight + yOffset - (rotSize / 2);
                            int yMin = yOffset + (rotSize / 2);
                            int ySeed = rand.Next(yMin, yMax);

                            snapshotSpriteObj.transform.localScale = new Vector3(size, size);

                            if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[0])
                            {
                                //normalise rotation
                                rotSeed = (int)(Mathf.Round(rotSeed / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot * Mathf.Sign(rotSeed));
                            }
                            else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[1])
                            {
                                //normalise rotation
                                rotSeed = (int)(Mathf.Round(rotSeed / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot * Mathf.Sign(rotSeed));
                                //put shape in the middle
                                xSeed = textureWidth / 2;
                            }
                            else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[2])
                            {
                                //double up shape on both sides
                                //get opposite xSeed
                                int x2 = textureWidth - xSeed;
                                //mirror rotation
                                snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, -rotSeed);
                                DrawShapeFromSnapshot(texture, x2, ySeed);
                            }
                            //else no symmetry

                            snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, rotSeed);

                            x0 = xSeed;
                            y0 = ySeed;

                            /*
                            Debug.Log("Rot seed (" + rotSeed + ")");
                            Debug.Log("Size seed (" + minSize + ", " + (int)(maxBossWidth * shapeSizeLimiter) + "): " + sizeSeed + " (" + size + "%)");
                            Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                            Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            */
                        }
                        break;
                    //width and height
                    case ShapeTypeName.RECT:
                    case ShapeTypeName.OVAL:
                    case ShapeTypeName.HALO:
                    case ShapeTypeName.SEMIOVAL:
                    case ShapeTypeName.DIAMOND:
                    case ShapeTypeName.ISOTRI:
                    case ShapeTypeName.IPENT:
                    case ShapeTypeName.IHEX:
                        {
                            int minWidth = maxBossWidth / 10;
                            int minHeight = maxBossHeight / 10;

                            int rotSeed = rand.Next(-180, 180);

                            float theta = rotSeed * Mathf.Deg2Rad;

                            int widthSeed = rand.Next(minWidth, (int)(maxBossWidth * shapeSizeLimiter));
                            int heightSeed = rand.Next(minHeight, (int)(maxBossHeight * shapeSizeLimiter));
                            float width = widthSeed / (float)maxBossWidth;
                            float height = heightSeed / (float)maxBossHeight;

                            int rotWidth = (int)(Mathf.Abs(heightSeed * Mathf.Sin(theta)) + Mathf.Abs(widthSeed * Mathf.Cos(theta)));
                            int rotHeight = (int)(Mathf.Abs(widthSeed * Mathf.Sin(theta)) + Mathf.Abs(heightSeed * Mathf.Cos(theta)));

                            int xMax = maxBossWidth + xOffset - (rotWidth / 2);
                            int xMin = xOffset + (rotWidth / 2);
                            int xSeed = rand.Next(xMin, xMax);

                            int yMax = maxBossHeight + yOffset - (rotHeight / 2);
                            int yMin = yOffset + (rotHeight / 2);
                            int ySeed = rand.Next(yMin, yMax);

                            snapshotSpriteObj.transform.localScale = new Vector3(width, height);

                            if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[0])
                            {
                                //normalise rotation
                                rotSeed = (int)(Mathf.Round(rotSeed / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot);
                            }
                            else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[1])
                            {
                                //normalise rotation
                                rotSeed = (int)(Mathf.Round(rotSeed / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot);
                                //put shape in the middle
                                xSeed = textureWidth / 2;
                            }
                            else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[2])
                            {
                                //double up shape on both sides
                                //get opposite xSeed
                                int x2 = textureWidth - xSeed;
                                //mirror rotation
                                snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, -rotSeed);
                                DrawShapeFromSnapshot(texture, x2, ySeed);
                            }

                            snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, rotSeed);

                            x0 = xSeed;
                            y0 = ySeed;

                            /*
                            Debug.Log("Rot seed (" + rotSeed + ")");
                            Debug.Log("Width seed (" + minWidth + ", " + (int)(maxBossWidth * shapeSizeLimiter) + "): " + widthSeed + " (" + width + "%)");
                            Debug.Log("Height seed (" + minHeight + ", " + (int)(maxBossHeight * shapeSizeLimiter) + "): " + heightSeed + " (" + height + "%)");
                            Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                            Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            */
                        };
                        break;
                    //width and height (sprite needs mirroring for symmetry)
                    case ShapeTypeName.RHOMBUS:
                    case ShapeTypeName.RANGLETRI:
                    case ShapeTypeName.SCALENETRI:
                        {
                            int minWidth = maxBossWidth / 10;
                            int minHeight = maxBossHeight / 10;

                            int rotSeed = rand.Next(-45, 45);

                            float theta = rotSeed * Mathf.Deg2Rad;

                            int widthSeed = rand.Next(minWidth, (int)(maxBossWidth * shapeSizeLimiter));
                            int heightSeed = rand.Next(minHeight, (int)(maxBossHeight * shapeSizeLimiter));
                            float width = widthSeed / (float)maxBossWidth;
                            float height = heightSeed / (float)maxBossHeight;

                            int rotWidth = (int)(Mathf.Abs(heightSeed * Mathf.Sin(theta)) + Mathf.Abs(widthSeed * Mathf.Cos(theta)));
                            int rotHeight = (int)(Mathf.Abs(widthSeed * Mathf.Sin(theta)) + Mathf.Abs(heightSeed * Mathf.Cos(theta)));

                            int xMax = maxBossWidth + xOffset - (rotWidth / 2);
                            int xMin = xOffset + (rotWidth / 2);
                            int xSeed = rand.Next(xMin, xMax);

                            int yMax = maxBossHeight + yOffset - (rotHeight / 2);
                            int yMin = yOffset + (rotHeight / 2);
                            int ySeed = rand.Next(yMin, yMax);

                            if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[0])
                            {
                                //double up shape on both sides
                                //get opposite xSeed
                                int x2 = textureWidth - xSeed;
                                //mirror rotation
                                snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, -rotSeed);
                                snapshotSpriteObj.transform.localScale = new Vector3(-width, height);
                                DrawShapeFromSnapshot(texture, x2, ySeed);
                            }

                            snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, rotSeed);
                            snapshotSpriteObj.transform.localScale = new Vector3(width, height);

                            x0 = xSeed;
                            y0 = ySeed;

                            /*
                            Debug.Log("Rot seed (" + rotSeed + ")");
                            Debug.Log("Width seed (" + minWidth + ", " + (int)(maxBossWidth * shapeSizeLimiter) + "): " + widthSeed + " (" + width + "%)");
                            Debug.Log("Height seed (" + minHeight + ", " + (int)(maxBossHeight * shapeSizeLimiter) + "): " + heightSeed + " (" + height + "%)");
                            Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                            Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            */
                        };
                        break;
                }

                DrawShapeFromSnapshot(texture, x0, y0);

                texture.Apply();

                bossSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                bossSprite.enabled = true;
            }
        }
    }

    /// <summary>
    /// Generate new colliders for the boss sprite, wiping any pre-existing ones
    /// </summary>
    private void GenerateColliders()
    {
        Destroy(bossSprite.gameObject.GetComponent<PolygonCollider2D>());

        bossSprite.gameObject.AddComponent<PolygonCollider2D>();
    }

    /// <summary>
    /// Randomly select the weapons that will be attached to the boss
    /// </summary>
    private void GenerateWeapons()
    {
        //clear previous weapons
        Weapon[] weapons = bossObj.GetComponentsInChildren<Weapon>();
        for (int i = 0; i < weapons.Length; i++)
        {
            Destroy(weapons[i].gameObject);
        }

        //TODO: randomise number of weapons

        for (int i = 0; i < numberOfWeapons; i++)
        {
            //pick random weapon type
            WeaponType weaponType;
            bool weaponPicked = false;

            int count = 0;
            int weaponTypeSeed;
            int index;
            int mask;

            do
            {
                weaponTypeSeed = rand.Next(0, weaponTypeMax);
                index = (int)(weaponTypeSeed / (float)weaponTypeMax * GeneratableWeapons.Length);
                weaponType = GeneratableWeapons[index];

                mask = 1 << (int)bossType.typeName;

                if (((int)weaponType.bossTypesWeaponWieldableBy & mask) == mask)
                {
                    weaponPicked = true;
                }
                count++;
                if (count > maxWeaponTypeAttempts)
                {
                    Debug.Log("Couldn't pick weapon for this boss type");
                    break;
                }
            } while (!weaponPicked);

            //skip this weapon if a weapon can't be found
            if (!weaponPicked)
            {
                continue;
            }

            //create weapon gameobject
            GameObject weapon = Instantiate(WeaponPrefab, bossObj.transform);

            //set up weapon sprite
            SpriteRenderer sr = weapon.GetComponentInChildren<SpriteRenderer>();
            sr.sprite = weaponType.sprite;
            //add collider
            sr.gameObject.AddComponent<PolygonCollider2D>();

            //if symmetric type 2, instantiate a mirror of the weapon
            GameObject weaponMirror = null;
            if (symmetricSeed >= symmetricMax * weaponType.symmetryProbBounds[0] && symmetricSeed < symmetricMax * weaponType.symmetryProbBounds[1])
            {
                weaponMirror = Instantiate(WeaponPrefab, weapon.transform);

                //set up weapon sprite
                SpriteRenderer srm = weaponMirror.GetComponentInChildren<SpriteRenderer>();
                srm.sprite = weaponType.sprite;
                //add collider
                srm.gameObject.AddComponent<PolygonCollider2D>();
            }

            //bool for whether weapon is placed on sprite correctly
            //if weapon can float ignore process and just pick first
            bool foundPosition = weaponType.canWeaponFloat;

            count = -1;
            int xSeed;
            int ySeed;
            int mirrorXSeed;

            do
            {
                count++;
                if (count > maxWeaponPosAttempts)
                {
                    Debug.Log("Could not find position for this weapon");
                    break;
                }

                if (symmetricSeed < symmetricMax * weaponType.symmetryProbBounds[0])
                {
                    xSeed = 0;
                }
                else
                {
                    xSeed = rand.Next((int)(-weaponXLimit * 100), (int)(weaponXLimit * 100));
                }

                ySeed = rand.Next((int)(-weaponYLimit * 100), (int)(weaponYLimit * 100));

                weapon.transform.position = new Vector3(xSeed / 100.0f, ySeed / 100.0f, -1f) + bossObj.transform.position;

                //if a raycast does not hit the boss sprite collider from this weapon position, try again
                if (!Physics2D.Raycast(weapon.transform.position, weapon.transform.forward, 1000, bossSpriteLayer))
                {
                    continue;
                }

                //if weapon is symmetrically mirrored, do the same check for mirror weapon
                if (weaponMirror != null)
                {
                    mirrorXSeed = -xSeed;
                    Debug.Log(mirrorXSeed);

                    weaponMirror.transform.position = new Vector3(mirrorXSeed / 100.0f, ySeed / 100.0f, -1f) + bossObj.transform.position;
                    Debug.Log(weaponMirror.transform.localPosition);

                    if (!Physics2D.Raycast(weaponMirror.transform.position, weapon.transform.forward, 1000, bossSpriteLayer))
                    {
                        continue;
                    }
                }

                foundPosition = true;
            } while (!foundPosition);
        }
    }
}
