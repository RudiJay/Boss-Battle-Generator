/* 
 * Copyright (C) 2018 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
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
[System.Flags]
public enum WeaponOrientationMode
{
    FIXEDFORWARD,
    FIXEDSIDEWAYS,
    FIXEDOTHERFORWARDS,
    FIXEDOTHER,
    ROTATABLE,
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

    [Header("Symmetry Multipliers")][Space(5)]
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

    [EnumFlags]
    public WeaponOrientationMode availableWeaponOrientations;

    [EnumFlags]
    public BossTypeName bossTypesWeaponWieldableBy;

    public bool canWeaponFloat;

    [Header("Symmetry")]
    [Space(5)]
    public float AsymmetricProbability;
    public float CentreXProbability;
    public float MirrorProbability;
}

public class GeneratorScript : MonoBehaviour
{
    public static GeneratorScript Instance;

    private System.Random rand;

    private GameObject bossObj;
    private SpriteRenderer bossSprite;

    private Camera spriteSnapshotCam;

    private GameObject snapshotSpriteObj;

    private GameObject background;

    private IEnumerator autoGenerator;
    private bool isAutoGenerating = false;
    WaitForSeconds autoGenerateWaitForSeconds;

    [Header("Interface")]
    [SerializeField]
    private float autoGeneratorDelay = 2.0f;
    [SerializeField][Tooltip("CAUTION: do not use unless testing sprite creation")]
    private bool DebugShowSpriteBorder = false;

    
    [Header("Generation Variables")]
    [SerializeField][Space(10)]
    private int seed;
    private int symmetryValue;

    [SerializeField]
    private int maxBossWidth = 500, maxBossHeight = 500;
    private int textureWidth, textureHeight;
    private int xOffset, yOffset;
    [SerializeField][Range(0,1)]
    private float shapeSizeLimiter = 0.75f; 
    [SerializeField]
    private float weaponXLimit = 3, weaponYLimit = 3;

    [SerializeField][Range(0, 1)]
    private float nonComplementaryColorChance = 0.5f, asymmetricColorChance = 0.25f;

    [Header("Randomisation Scales")]
    [SerializeField][Space(10)]
    private int bossTypeMax = 100;
    [SerializeField]
    private int symmetryMax = 100, shapeComplexityMax = 100, shapeMax = 100, perlinColorScaleMin = 100, perlinColorScaleMax = 100, 
        weaponQuantityMax = 100, weaponTypeMax = 100, weaponOrientationMax = 100;

    [Header("Boss Type")]
    [SerializeField][Space(10)]
    private BossType[] BossTypeVariables;
    private BossType bossType;

    [Header("Appearance")]
    [SerializeField][Space(10)]
    private ShapeType[] SpriteGenerationShapes;
    private int spriteShapeComplexity = 3;

    private int colorQuantity = 3;
    private Color[] colorPalette;
    [SerializeField]
    private bool useColorSchemeForBackground = false;

    [Header("Weapons")]
    [SerializeField][Space(10)]
    private int weaponQuantity = 2;
    [SerializeField]
    private GameObject WeaponPrefab;
    [SerializeField]
    private LayerMask bossSpriteLayer, weaponSpriteLayer;
    [SerializeField]
    private int maxWeaponTypeAttempts = 10, maxWeaponOrientationAttempts = 10, maxWeaponPosAttempts = 15;
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

        autoGenerateWaitForSeconds = new WaitForSeconds(autoGeneratorDelay);
        autoGenerator = AutoGenerateBossFights();

        bossObj = GameObject.FindWithTag("Boss");
        if (bossObj)
        {
            bossSprite = bossObj.GetComponentInChildren<SpriteRenderer>();
        }

        spriteSnapshotCam = GameObject.FindWithTag("SpriteSnapshotCam").GetComponent<Camera>();

        snapshotSpriteObj = GameObject.FindWithTag("SnapshotSpriteObj");

        background = GameObject.FindWithTag("Background");

        //calculate determinant sizes
        textureWidth = (int)(maxBossWidth * 1.25f);
        textureHeight = (int)(maxBossHeight * 1.25f);
        xOffset = (textureWidth - maxBossWidth) / 2;
        yOffset = (textureHeight - maxBossHeight) / 2;
    }

    private void Update()
    {
        if (Input.GetButtonDown("GenerateBoss"))
        {
            GenerateBossFight(true);
        }

        if (Input.GetButtonDown("ToggleUI"))
        {
            GeneratorUI.Instance.ToggleUI();
        }

        //Toggle auto generate input
        if (Input.GetButtonDown("ToggleAutoGenerate"))
        {
            isAutoGenerating = !isAutoGenerating;

            if (isAutoGenerating)
            {
                StartCoroutine(autoGenerator);
            }
            else
            {
                StopCoroutine(autoGenerator);
            }
        }
    }

    /// <summary>
    /// Generates a new boss repeatedly with a delay
    /// </summary>
    private IEnumerator AutoGenerateBossFights()
    {
        while (true)
        {
            GenerateBossFight(true);

            yield return autoGenerateWaitForSeconds;
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
        seed = inSeed;

        rand = new System.Random(seed);

        GenerateBossFight(false);
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

            GenerateRandomSymmetryScore();

            GenerateColorScheme();

            GenerateBackground();

            GenerateSprite();
            
            GenerateColliders();

            GenerateWeapons();

            GeneratorUI.Instance.ToggleGeneratingInProgressLabel(false);
        }
    }

    /// <summary>
    /// Assigns the value of the Boss Type from the UI, and randomises it if set to "Random"
    /// </summary>
    private void SetBossType()
    {
        BossTypeName typeName;
        typeName = GeneratorUI.Instance.GetBossTypeName();

        if (typeName == BossTypeName.Random)
        {
            int typeSeed = rand.Next(0, bossTypeMax);
            int index = (int)(typeSeed / (float)bossTypeMax * (System.Enum.GetNames(typeof(BossTypeName)).Length - 1)) + 1; //-1 and +1 to avoid setting boss type to RANDOM again
            typeName = (BossTypeName)index;
            GeneratorUI.Instance.ShowRandomBossType(typeName.ToString());
        }

        bossType = System.Array.Find<BossType>(BossTypeVariables, BossTypeVariables => BossTypeVariables.typeName == typeName);
    }

    /// <summary>
    /// Randomly assigns a new symmetry score
    /// </summary>
    private void GenerateRandomSymmetryScore()
    {
        symmetryValue = rand.Next(0, symmetryMax);
        //Debug.Log("Symmetric seed (0, " + symmetryMax + "): " + symmetryValue);
    }

    /// <summary>
    /// Randomly generates the color scheme that will be used for the boss
    /// </summary>
    private void GenerateColorScheme()
    {
        colorQuantity = rand.Next(1, 5);
        colorPalette = new Color[colorQuantity];

        float hue = rand.Next(0, 100) / 100.0f;
        float saturation = rand.Next(0, 100) / 100.0f;
        float brightness = rand.Next(0, 100) / 100.0f;

        colorPalette[0] = Color.HSVToRGB(hue, saturation, brightness);

        GenerateRandomSymmetryScore();
        bool useComplementaryColors = symmetryValue / (float)symmetryMax > nonComplementaryColorChance;
        float offset = 0;
        for (int i = 1; i < colorQuantity; i++)
        {
            //fourth color is always random
            if (useComplementaryColors && i < 3)
            {
                //determine which color offset to use 
                //(using split complementary for three colours)
                if (i == 1)
                {
                    if (colorQuantity == 2)
                    {
                        //complementary
                        offset = 0.5f;
                    }
                    else
                    {
                        //first split complementary
                        offset = 5f / 12f;
                    }
                }
                else if (i == 2)
                {
                    //second split complementary
                    offset = 7f / 12f;
                }

                hue = Mathf.Abs(hue + offset - 1);
            }
            else
            {
                hue = rand.Next(0, 100) / 100.0f;
                saturation = rand.Next(0, 100) / 100.0f;
                brightness = rand.Next(0, 100) / 100.0f;
            }

            colorPalette[i] = Color.HSVToRGB(hue, saturation, brightness);
        }
    }

    /// <summary>
    /// Applies a randomly generated transformation to the background image
    /// </summary>
    public void GenerateBackground()
    {
        if (background)
        {
            Material mat = background.GetComponent<Renderer>().material;
            Color bgColor;

            if (useColorSchemeForBackground)
            {
                bgColor = colorPalette[rand.Next(0, colorQuantity)];
            }
            else
            {
                int redSeed = rand.Next(0, 255);
                int greenSeed = rand.Next(0, 255);
                int blueSeed = rand.Next(0, 255);

                bgColor = new Color(redSeed / 255f, greenSeed / 255f, blueSeed / 255f);
            }

            mat.SetColor("_Color", bgColor);

            int xSeed = rand.Next(-100, 100);
            int ySeed = rand.Next(-100, 100);

            mat.mainTextureOffset = new Vector2(xSeed / 100f, ySeed / 100f);
        }
    }

    /// <summary>
    /// Takes in the sprite shape generated and applies the color scheme to it
    /// </summary>
    /// <param name="texture"></param>
    private void PaintSpriteColor(Texture2D texture)
    {
        float scaleX = rand.Next(perlinColorScaleMin, perlinColorScaleMax);
        float scaleY = rand.Next(perlinColorScaleMin, perlinColorScaleMax);

        bool symmetricColor = false;
        GenerateRandomSymmetryScore();
        if (symmetryValue / (float)symmetryMax > asymmetricColorChance)
        {
            symmetricColor = true;
        }

        float noiseColorBoundaryWidth = 1 / (float)colorQuantity;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                if (texture.GetPixel(x, y) == Color.white)
                {
                    float xCoord = x;
                    float yCoord = y;

                    if (symmetricColor)
                    {
                        xCoord = Mathf.Abs(x - texture.width / 2.0f);
                    }

                    float noise = Mathf.PerlinNoise(xCoord / scaleX, yCoord / scaleY);

                    texture.SetPixel(x, y, colorPalette[(int)(noise / noiseColorBoundaryWidth)]);
                }
            }
        }
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

            //make sure there is a complexity curve
            if (bossType.spriteComplexityCurve.length == 0)
            {
                Debug.Log("ERROR: Missing complexity curve");
                return;
            }

            //generate sprite complexity
            spriteShapeComplexity = Mathf.RoundToInt(bossType.spriteComplexityCurve.Evaluate(rand.Next(0, shapeComplexityMax) / (float)shapeComplexityMax));
            //Debug.Log("Sprite Shape Complexity: " + spriteShapeComplexity);
            //make sure there is at least one shape
            spriteShapeComplexity = Mathf.Max(spriteShapeComplexity, 1);

            for (int i = 0; i < spriteShapeComplexity; i++)
            {
                GenerateRandomSymmetryScore(); //symmetry score must be calculated again for every shape so they don't all do the same thing

                int shapeSeed = rand.Next(0, shapeMax);                
                //Debug.Log("Shape seed (0, " + shapeMax + "): " + shapeSeed);
                
                snapshotSpriteObj.transform.rotation = Quaternion.identity;

                ShapeType spriteShape;
                int index = (int)(shapeSeed / (float)shapeMax * SpriteGenerationShapes.Length);
                spriteShape = SpriteGenerationShapes[index];

                snapshotSprite.sprite = spriteShape.sprite;

                //set width of shape
                int minWidth = maxBossWidth / 10;
                int widthValue = rand.Next(minWidth, (int)(maxBossWidth * shapeSizeLimiter));
                float objWidth = widthValue / (float)maxBossWidth;

                int heightValue;
                float objHeight;
                bool generateHeight = spriteShape.twoDimensionSizeGeneration;
                //if the shape is scaled in two dimensions instead of just one, set height
                if (generateHeight)
                {
                    int minHeight = maxBossHeight / 10;
                    heightValue = rand.Next(minHeight, (int)(maxBossHeight * shapeSizeLimiter));
                    objHeight = heightValue / (float)maxBossHeight;
                }
                else
                {
                    heightValue = widthValue;
                    objHeight = objWidth;
                }

                int rotWidth;
                int rotHeight;
                int rotValue = 0;
                //if the shape is to be rotated, generate rotation
                if (spriteShape.generateRotation)
                {
                    rotValue = rand.Next(-180, 180);
                    float theta = rotValue * Mathf.Deg2Rad;

                    //get the bounding width of the rotated shape
                    rotWidth = (int)(Mathf.Abs(heightValue * Mathf.Sin(theta)) + Mathf.Abs(widthValue * Mathf.Cos(theta)));
                    rotHeight = (int)(Mathf.Abs(widthValue * Mathf.Sin(theta)) + Mathf.Abs(heightValue * Mathf.Cos(theta)));
                }
                else
                {
                    rotWidth = widthValue;
                    rotHeight = heightValue;
                }

                //generate x and y coordinate to place shape at
                int xMax = maxBossWidth + xOffset - (rotWidth / 2);
                int xMin = xOffset + (rotWidth / 2);
                int xValue = rand.Next(xMin, xMax);

                int yMax = maxBossHeight + yOffset - (rotHeight / 2);
                int yMin = yOffset + (rotHeight / 2);
                int yValue = rand.Next(yMin, yMax);

                snapshotSpriteObj.transform.localScale = new Vector3(objWidth, objHeight);

                //calculate probability bounds of each symmetry type
                float asymmetric = spriteShape.AsymmetricProbability * bossType.asymmetricProbabilityMultiplier;
                float normaliseRot = spriteShape.NormaliseRotProbability * bossType.normaliseRotProbabilityMultiplier;
                float centreX = spriteShape.CentreXAndNormaliseRotProbability * bossType.centreXProbabilityMultiplier;
                float mirror = spriteShape.MirrorProbability * bossType.mirrorProbabilityMultiplier;
                float symmetryProbabilityTotal = asymmetric + normaliseRot + centreX + mirror;
                float asymmetricMax = asymmetric / symmetryProbabilityTotal;
                float normaliseRotMax = asymmetricMax + (normaliseRot / symmetryProbabilityTotal);
                float centreXMax = normaliseRotMax + (centreX / symmetryProbabilityTotal);
                float mirrorMax = centreXMax + (mirror / symmetryProbabilityTotal);

                //determine which symmetry probability bound the shape belongs in
                if (symmetryValue >= asymmetricMax * symmetryMax && symmetryValue < normaliseRotMax * symmetryMax && spriteShape.generateRotation)
                {
                    //normalise rotation
                    rotValue = (int)(Mathf.Round(rotValue / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot * Mathf.Sign(rotValue));
                }
                else if (symmetryValue >= normaliseRotMax * symmetryMax && symmetryValue < centreXMax * symmetryMax)
                {
                    //normalise rotation
                    rotValue = spriteShape.generateRotation ? (int)(Mathf.Round(rotValue / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot * Mathf.Sign(rotValue)) : 0;
                    //centre shape in x axis
                    xValue = textureWidth / 2;
                }
                else if (symmetryValue >= centreXMax * symmetryMax && symmetryValue < mirrorMax * symmetryMax)
                {
                    //get opposite x value
                    int x2 = textureWidth - xValue;

                    //transform the source shape gameobject with mirror rotation and scale
                    snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, -rotValue);
                    snapshotSpriteObj.transform.localScale = new Vector3(-objWidth, objHeight);
                    //draw mirror image early
                    DrawShapeFromSnapshot(texture, x2, yValue);
                }

                //set source shape gameobject rotation and scale
                snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, rotValue);
                snapshotSpriteObj.transform.localScale = new Vector3(objWidth, objHeight);

                DrawShapeFromSnapshot(texture, xValue, yValue);
            }

            PaintSpriteColor(texture);

            texture.Apply();

            bossSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            bossSprite.enabled = true;
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

    private int GenerateWeaponTypeIndex()
    {
        WeaponType weaponType;

        bool weaponPicked = false;
        int count = 0;
        int weaponTypeSeed;
        int weaponTypeIndex;
        int bossTypeMask;

        do
        {
            weaponTypeSeed = rand.Next(0, weaponTypeMax);
            weaponTypeIndex = (int)(weaponTypeSeed / (float)weaponTypeMax * GeneratableWeapons.Length);
            weaponType = GeneratableWeapons[weaponTypeIndex];

            bossTypeMask = 1 << (int)bossType.typeName;

            count++;
            if (((int)weaponType.bossTypesWeaponWieldableBy & bossTypeMask) == bossTypeMask)
            {
                return weaponTypeIndex;
            }
            else if (count > maxWeaponTypeAttempts)
            {
                Debug.Log("Couldn't pick weapon for this boss type");
                break;
            }
        } while (!weaponPicked);

        return -1;
    }

    private int GenerateWeaponOrientationModeIndex(WeaponType weaponType)
    {
        WeaponOrientationMode orientationMode;

        bool orientationPicked = false;
        int weaponOrientationSeed;
        int orientationIndex;
        int orientationMask;
        int count = 0;

        do
        {
            weaponOrientationSeed = rand.Next(0, weaponOrientationMax);
            orientationIndex = (int)(weaponOrientationSeed / (float)weaponOrientationMax * System.Enum.GetNames(typeof(WeaponOrientationMode)).Length);
            orientationMode = (WeaponOrientationMode)orientationIndex;

            orientationMask = 1 << (int)orientationMode;

            count++;
            if (((int)weaponType.availableWeaponOrientations & orientationMask) == orientationMask)
            {
                //calculate probability boundaries of current weapon centring in X-axis for symmetry
                float asymmetric = weaponType.AsymmetricProbability * bossType.asymmetricProbabilityMultiplier;
                float centreX = weaponType.CentreXProbability * bossType.centreXProbabilityMultiplier;
                float mirror = weaponType.MirrorProbability * bossType.mirrorProbabilityMultiplier;
                float symmetryProbabilityTotal = asymmetric + centreX + mirror;
                float centreXMin = asymmetric / symmetryProbabilityTotal;
                float centreXMax = (asymmetric + centreX) / symmetryProbabilityTotal;

                //if symmetry is centred, do not allow non symmetrical orientation modes
                if (symmetryValue >= centreXMin * symmetryMax && symmetryValue < centreXMax * symmetryMax)
                {
                    if (orientationIndex != (int)WeaponOrientationMode.FIXEDSIDEWAYS &&
                        orientationIndex != (int)WeaponOrientationMode.FIXEDOTHERFORWARDS &&
                        orientationIndex != (int)WeaponOrientationMode.FIXEDOTHER)
                    {
                        return orientationIndex;
                    }
                }
                else
                {
                    return orientationIndex;
                }
            }
            else if (count > maxWeaponOrientationAttempts)
            {
                Debug.Log("Couldn't pick orientation for this weapon");
                break;
            }
        } while (!orientationPicked);

        return -1;
    }

    private bool GenerateWeaponPosition(WeaponType weaponType, Transform weaponTransform, Transform mirrorWeaponTransform = null)
    {
        //bool for whether weapon is placed on sprite correctly
        //if weapon can float ignore process and just pick first
        bool foundPosition = weaponType.canWeaponFloat;

        int count = -1;
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

            //calculate probability bounds for centring the current weapon in X-axis
            float asymmetric = weaponType.AsymmetricProbability * bossType.asymmetricProbabilityMultiplier;
            float centreX = weaponType.CentreXProbability * bossType.centreXProbabilityMultiplier;
            float mirror = weaponType.MirrorProbability * bossType.mirrorProbabilityMultiplier;
            float symmetryProbabilityTotal = asymmetric + centreX + mirror;
            float centreXMin = asymmetric / symmetryProbabilityTotal;
            float centreXMax = (asymmetric + centreX) / symmetryProbabilityTotal;

            //if current weapon should be centred in X axis for symmetry, do so
            if (symmetryValue >= centreXMin * symmetryMax && symmetryValue < centreXMax * symmetryMax)
            {
                xSeed = 0;
            }
            else
            {
                xSeed = rand.Next((int)(-weaponXLimit * 100), (int)(weaponXLimit * 100));
            }

            ySeed = rand.Next((int)(-weaponYLimit * 100), (int)(weaponYLimit * 100));

            weaponTransform.position = new Vector3(xSeed / 100.0f, ySeed / 100.0f, -1f) + bossObj.transform.position;

            //if a raycast does not hit the boss sprite collider from this weapon position, try again
            if (!Physics2D.Raycast(weaponTransform.position, weaponTransform.forward, 1000, bossSpriteLayer) && !foundPosition)
            {
                continue;
            }

            if (weaponTransform.GetComponent<Weapon>().CheckIfCollidingWithOtherWeapons())
            {
                //TODO: Fix test so removed correctly
                //continue;
            }

            //if weapon uses mirror symmetry, do the same check for mirror weapon
            if (mirrorWeaponTransform != null)
            {
                mirrorXSeed = -xSeed;

                mirrorWeaponTransform.position = new Vector3(mirrorXSeed / 100.0f, ySeed / 100.0f, -1f) + bossObj.transform.position;

                if (!Physics2D.Raycast(mirrorWeaponTransform.position, mirrorWeaponTransform.forward, 1000, bossSpriteLayer) && !foundPosition)
                {
                    continue;
                }

                if (mirrorWeaponTransform.GetComponent<Weapon>().CheckIfCollidingWithOtherWeapons())
                {
                    //TODO: Fix test so removed correctly
                    //continue;
                }
            }

            //foundPosition = true; --unneeded because method is exited anyway
            return true;
        } while (!foundPosition);

        return false;
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

        //make sure there is a complexity curve
        if (bossType.weaponQuantityCurve.length == 0)
        {
            Debug.Log("ERROR: Missing weapon quantity curve");
            return;
        }

        //generate number of weapons
        weaponQuantity = Mathf.RoundToInt(bossType.weaponQuantityCurve.Evaluate(rand.Next(0, weaponQuantityMax) / (float)weaponQuantityMax));
        //Debug.Log("Number of weapons: " + weaponQuantity);

        for (int i = 0; i < weaponQuantity; i++)
        {
            GenerateRandomSymmetryScore(); //symmetry score must be calculated again for each new weapon so they don't all do the same thing.

            //pick random weapon type
            int weaponTypeIndex = GenerateWeaponTypeIndex();

            //skip this weapon if a weapon can't be found
            if (weaponTypeIndex == -1)
            {
                continue;
            }
            WeaponType weaponType = GeneratableWeapons[weaponTypeIndex];            

            //create weapon gameobject
            GameObject weapon = Instantiate(WeaponPrefab, bossObj.transform);
            Weapon weaponComponent = weapon.GetComponent<Weapon>();

            //set up weapon sprite
            SpriteRenderer sr = weapon.GetComponentInChildren<SpriteRenderer>();
            sr.sprite = weaponType.sprite;
            //set weapon size
            weapon.transform.localScale = new Vector3(weaponType.size, weaponType.size);
            //add collider
            sr.gameObject.AddComponent<PolygonCollider2D>();
            sr.gameObject.GetComponent<PolygonCollider2D>().isTrigger = true;

            //set weapon orientation mode
            int orientationModeIndex = GenerateWeaponOrientationModeIndex(weaponType);

            //if setting orientation mode failed, destroy weapon and move on
            if (orientationModeIndex == -1)
            {
                Destroy(weapon);
                continue;
            }
            WeaponOrientationMode orientationMode = (WeaponOrientationMode)orientationModeIndex;
            //Debug.Log(orientationMode);
            weaponComponent.currentOrientationMode = orientationMode;

            //check symmetry score and if weapon should use mirror symmetry instantiate new weapon object
            GameObject mirrorWeapon = null;
            Weapon mirrorWeaponComponent = null;

            //calculate probability bounds for weapon being mirrored symmetrically
            float asymmetric = weaponType.AsymmetricProbability * bossType.asymmetricProbabilityMultiplier;
            float centreX = weaponType.CentreXProbability * bossType.centreXProbabilityMultiplier;
            float mirror = weaponType.MirrorProbability * bossType.mirrorProbabilityMultiplier;
            float symmetryProbabilitiesTotal = asymmetric + centreX + mirror;
            float mirrorSymmetryMin = (asymmetric + centreX) / symmetryProbabilitiesTotal;
            float mirrorSymmetryMax = 1.0f;

            if (symmetryValue >= mirrorSymmetryMin * symmetryMax && symmetryValue < mirrorSymmetryMax * symmetryMax)
            {
                mirrorWeapon = Instantiate(WeaponPrefab, bossObj.transform);
                mirrorWeaponComponent = mirrorWeapon.GetComponent<Weapon>();

                //set up weapon sprite
                SpriteRenderer mirrorsr = mirrorWeapon.GetComponentInChildren<SpriteRenderer>();
                mirrorsr.sprite = weaponType.sprite;
                //set weapon size
                mirrorWeapon.transform.localScale = new Vector3(weaponType.size, weaponType.size);
                //add collider
                mirrorsr.gameObject.AddComponent<PolygonCollider2D>();
                mirrorsr.gameObject.GetComponent<PolygonCollider2D>().isTrigger = true;
                //set weapon orientation mode
                mirrorWeaponComponent.currentOrientationMode = orientationMode;
            }

            //position weapon on boss sprite 
            bool weaponPlaced = (mirrorWeapon == null) ? GenerateWeaponPosition(weaponType, weapon.transform) : GenerateWeaponPosition(weaponType, weapon.transform, mirrorWeapon.transform);

            //if weapon could not find a position, destroy weapons
            if (!weaponPlaced)
            {
                Destroy(weapon);
                Destroy(mirrorWeapon);
                continue;
            }

            //set weapon rotation according to orientationMode
            switch (orientationMode)
            {
                case WeaponOrientationMode.FIXEDFORWARD:
                case WeaponOrientationMode.NONORIENTED:
                case WeaponOrientationMode.ROTATABLE:
                    break;
                case WeaponOrientationMode.FIXEDSIDEWAYS:
                    {
                        weaponComponent.SetWeaponRotation(90 * Mathf.Sign(weapon.transform.position.x));

                        if (mirrorWeaponComponent != null)
                        {
                            mirrorWeaponComponent.SetWeaponRotation(90 * Mathf.Sign(mirrorWeapon.transform.position.x));
                        }
                    }
                    break;
                case WeaponOrientationMode.FIXEDOTHERFORWARDS:
                    {
                        int randomOrientation = rand.Next(0, 90);
                        weaponComponent.SetWeaponRotation(randomOrientation * Mathf.Sign(weapon.transform.position.x));

                        if (mirrorWeaponComponent != null)
                        {
                            mirrorWeaponComponent.SetWeaponRotation(randomOrientation * Mathf.Sign(mirrorWeapon.transform.position.x));
                        }
                    }
                    break;
                case WeaponOrientationMode.FIXEDOTHER:
                    {
                        int randomOrientation = rand.Next(0, 180);
                        weaponComponent.SetWeaponRotation(randomOrientation * Mathf.Sign(weapon.transform.position.x));

                        if (mirrorWeaponComponent != null)
                        {
                            mirrorWeaponComponent.SetWeaponRotation(randomOrientation * Mathf.Sign(mirrorWeapon.transform.position.x));
                        }
                    }
                    break;
            }
        }
    }
}