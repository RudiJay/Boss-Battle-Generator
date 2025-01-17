﻿/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is applied to the boss generator prefab object and manages all generation-specific functions in scene
/// </summary>
public class GeneratorScript : MonoBehaviour
{
    public static GeneratorScript Instance;

    private bool generatorActive = true;

    /// <summary>
    /// variable containing an instance of the Random engine for the boss generator
    /// Stored to avoid outside calls affecting sequence
    /// </summary>
    private System.Random rand;

    private GameObject bossObj;
    private SpriteRenderer bossSprite;
    private BossLogic bossLogic;
    private WeaponManager weaponManager;

    private Camera spriteSnapshotCam; //camera for taking snapshot of sprite shape for sprite generation
    private GameObject snapshotSpriteObj; //sprite shape template for taking snapshot of and including in boss sprite

    private GameObject background; //the gameobject holding the background image

    private IEnumerator bossGeneration; //coroutine instance for boss generation sequence
    private bool generationInProgress = false;
    private WaitForSeconds generationStepDelayTime;

    private IEnumerator autoGenerator; //coroutine instance for starting boss generation automatically
    private bool isAutoGenerating = false;
    private WaitForSeconds autoGenerateWaitForSeconds;

    [Header("Interface")]
    [SerializeField]
    private float generationStepDelay = 0.5f; //time in seconds to wait between steps of boss generation
    [SerializeField]
    private float autoGeneratorDelay = 2.0f; //time in seconds to wait between automatically restarting boss generation

    
    [Header("Generation Variables")]
    [SerializeField][Space(10)]
    private int seed; //the seed for boss generation random values
    private int symmetryValue; //variable containing current random value to use for symmetry calculations

    [SerializeField]
    private int maxBossWidth = 500, maxBossHeight = 500;
    private int textureWidth, textureHeight;
    private int xOffset, yOffset;
    [SerializeField][Range(0,1)]
    private float shapeSizeLimiter = 0.75f; 
    [SerializeField]
    private float weaponXLimit = 3, weaponYLimit = 3;
    
    [SerializeField]
    private int maxProbabilityValue = 100;

    [Header("Boss Type")]
    [SerializeField][Space(10)]
    private BossType[] bossTypeVariables;
    private BossType bossType;

    private bool spriteGenerationComplete = false;
    [Header("Appearance")]
    [SerializeField][Space(10)]
    private ShapeType[] spriteGenerationShapes;
    [SerializeField]
    private float minShapeFractionOfMax = 7.5f; //denominator for the minimum fraction of the max size a shape can be scaled to
    private int spriteShapeComplexity = 3; //number of basic shapes a boss sprite is made up of

    private int colorQuantity = 3;
    private Color[] colorPalette;
    [SerializeField]
    [Range(0, 1)]
    private float nonComplementaryColorChance = 0.5f, asymmetricPatternChance = 0.25f; //chance of colours not being complementary, or pattern being asymmetrical
    [SerializeField]
    private int perlinColorScaleMin = 25, perlinColorScaleMax = 100; //min and max values for random scaling of perlin noise map for pattern gen
    [SerializeField]
    private int perlinOriginPointResolution = 100; //the max value for random origin on perlin noise map in x and y. Min is negative value.
    [SerializeField]
    private int bossSpriteOutlineWidth = 1, weaponSpriteOutlineWidth = 1; //number of pixels diameter for outlines around boss and weapon sprites
    [SerializeField]
    [Range(0, 1)]
    private float weaponBrightnessTintAmount = 0.25f; //amount to tint brightness of weapon colour compared to color palette
    [SerializeField]
    private bool useColorSchemeForBackground = false; //whether boss color scheme is used to tint background image or a random colour is used

    private bool weaponGenerationComplete = false;
    private bool weaponPositioningProcessComplete = false;
    private bool weaponPositionFound = false;
    [Header("Weapons")]
    [SerializeField][Space(10)]
    private LayerMask bossSpriteLayer;
    [SerializeField]
    private int maxWeaponTypeAttempts = 10, maxWeaponOrientationAttempts = 10, maxWeaponPosAttempts = 15; //number of times to attempt weapon gen loops before breaking out
    [SerializeField]
    private WeaponType[] generatableWeapons; //array of preset weapons that are possible to be placed on a boss
    private List<Weapon> bossWeapons; //list of weapons placed on current boss
    private int weaponQuantity = 2; //quantity of weapons the boss has, not counting mirror pairs

    private bool attackGenerationComplete = false;
    [Header("Attacks")]
    [SerializeField]
    [Space(10)]
    private int attackQuantityMax = 10; //maximum quantity of attack types a boss can have
    [SerializeField]
    private int attackSequenceLengthMin = 1, attackSequenceLengthMax = 15; //min and max attack sequence length
    [SerializeField]
    private ScriptableObject[] generatableAttackTypes; //array of preset attacks that are possible for a boss to pick from
    private List<IAttackType> bossAttackTypes; //list of attack types current boss knows
    private List<IAttackType> bossAttackSequence; //list containing the ordered attack sequence the current boss performs
    private int attackQuantity = 3; //quantity of attack types the current boss has

    private bool movementPatternGenerationComplete = false;
    [Header("Movement Patterns")]
    [SerializeField]
    [Space(10)]
    private List<VelocityCurveType> availableAccelerationTypes; //list containing preset velocity curves the boss can apply to movement patterns
    [SerializeField]
    private List<MovementPatternType> generatedMovementPatterns; //list containing the movement patterns the current boss uses
    private List<MovementPatternType> bossMovementPatternSequence; //list containing ordered movement pattern the current boss performs
    private int movementPatternQuantity = 5; //quantity of movement patterns the current boss has
    
    [SerializeField]
    private int movementPatternQuantityMin = 1, movementPatternQuantityMax = 5;
    [SerializeField]
    private int movementPatternSequenceLengthMin = 1, movementPatternSequenceLengthMax = 15;
    [SerializeField]
    private AnimationCurve movementsPerPatternProbabilityWeighting; //curve determining the probability weightings of values for the number of movements per pattern
    [SerializeField]
    private int destinationPointCountMin = 2, destinationPointCountMax = 9; //min and max number of points the can boss move between during a movement pattern
    [SerializeField]
    private float minDestinationX = -10.5f, minDestinationY = -8.0f, maxDestinationX = 10.5f, maxDestinationY = 1.5f; //min and max x and y for generated destination points
    [SerializeField]
    private float constrainXAxisChance = 0.15f, constrainYAxisChance = 0.3f; //chance of x or y axis being constrained
    [SerializeField]
    private float includeStartPointChance = 0.5f; //chance of start point being included in destination points
    [SerializeField]
    private float randomlyDecideNextDestinationChance = 0.0f; //chance of randomly deciding destinations in a pattern instead of in a set order
    [SerializeField]
    private AnimationCurve destinationWaitTimeProbabilityWeighting; //probability weighting for values of amount of time to wait after reaching a destination point

    [Header("Boss Stats")]
    [SerializeField]
    private int minBossLifePoints = 75;
    [SerializeField]
    private int maxBossLifePoints = 350; //min and max for boss life points
    [SerializeField]
    private float minBossSpeed = 2f, maxBossSpeed = 10f; //min and max for speed modifier of boss movement pattern movement

    /// <summary>
    /// Function updating variables for whether generator is currently active or not
    /// </summary>
    public void CheckGeneratorActive()
    {
        generatorActive = GameManager.Instance.GetGeneratorActive();
    }

    private void Awake()
    {
        //initialise singleton pattern
        Instance = this;
    }

    private void Start()
    {
        CheckGeneratorActive();
        //apply as listener to generator activation on gamemanager
        GameManager.Instance.RegisterAsGeneratorListener(CheckGeneratorActive);

        //initialise references
        rand = new System.Random();

        generationStepDelayTime = new WaitForSeconds(generationStepDelay);

        autoGenerateWaitForSeconds = new WaitForSeconds(autoGeneratorDelay);
        autoGenerator = AutoGenerateBossFights();

        bossObj = GameObject.FindWithTag("Boss");
        if (bossObj)
        {
            bossSprite = bossObj.GetComponentInChildren<SpriteRenderer>();
            bossLogic = bossObj.GetComponent<BossLogic>();
            weaponManager = bossObj.GetComponent<WeaponManager>();
        }

        spriteSnapshotCam = GameObject.FindWithTag("SpriteSnapshotCam").GetComponent<Camera>();
        snapshotSpriteObj = GameObject.FindWithTag("SnapshotSpriteObj");
        background = GameObject.FindWithTag("Background");
        
        bossWeapons = new List<Weapon>();
        bossAttackTypes = new List<IAttackType>();
        bossAttackSequence = new List<IAttackType>();
        generatedMovementPatterns = new List<MovementPatternType>();
        bossMovementPatternSequence = new List<MovementPatternType>();

        //calculate determinant sprite size variables
        textureWidth = (int)(maxBossWidth * 1.25f);
        textureHeight = (int)(maxBossHeight * 1.25f);
        xOffset = (textureWidth - maxBossWidth) / 2;
        yOffset = (textureHeight - maxBossHeight) / 2;
    }

    private void Update()
    {
        if (generatorActive)
        {
            if (Input.GetButtonDown("GenerateBoss"))
            {
                GenerateBossFight(true);
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

        UIManager.Instance.UpdateSeedUI(seed.ToString());

        rand = new System.Random(seed);
    }

    /// <summary>
    /// Sets the seed for the random boss fight generation
    /// </summary>
    /// <param name="inSeed">the number to set the seed to</param>
    public void SetSeed(int inSeed)
    {
        if (seed != inSeed)
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
        if (bossSprite && !generationInProgress && generatorActive)
        {
            GameManager.Instance.StopAttackSequence();
            ProjectileManager.Instance.DisableAllProjectiles();
            UIManager.Instance.SetBossLifebarActive(false);

            bossGeneration = GenerationProcess(generateNewSeed);

            StartCoroutine(bossGeneration);
        }
    }

    /// <summary>
    /// Stop boss generation sequence coroutine
    /// </summary>
    private void StopBossGeneration()
    {
        StopCoroutine(bossGeneration);

        generationInProgress = false;
    }

    /// <summary>
    /// Coroutine for completing boss generation sequence over multiple frames
    /// </summary>
    /// <param name="generateNewSeed">Whether a new boss seed needs to be generated</param>
    private IEnumerator GenerationProcess(bool generateNewSeed)
    {
        generationInProgress = true;
        
        spriteGenerationComplete = false;
        weaponGenerationComplete = false;
        attackGenerationComplete = false;
        movementPatternGenerationComplete = false;

        UIManager.Instance.SetGeneratingInProgressLabelEnabled(true);

        yield return null;

        //clean up potential previous boss
        ClearWeapons();
        ClearAttacks();
        ClearMovementPatterns();
        UIManager.Instance.ResetAttackUI();
        UIManager.Instance.ResetMovementPatternUI();
        UIManager.Instance.ResetBossStatUI();

        yield return null;

        if (generateNewSeed)
        {
            GenerateSeed();
        }

        SetBossType();

        yield return null;

        GenerateColorPalette();

        GenerateBackground();

        yield return null;

        //start sprite generation sequence
        StartCoroutine(GenerateSprite());
        //wait until sequence is complete before continuing
        while (!spriteGenerationComplete)
        {
            yield return null;
        }

        GenerateColliders();

        yield return null;
        
        StartCoroutine(GenerateWeapons());
        //wait until sequence is complete before continuing
        while (!weaponGenerationComplete)
        {
            yield return null;
        }

        StartCoroutine(GenerateAttacks());
        //wait until sequence is complete before continuing
        while (!attackGenerationComplete)
        {
            yield return null;
        }

        UIManager.Instance.SetAttackQuantity(attackQuantity); //not currently displayed on UI, useful for debug

        GenerateAttackSequence();

        yield return null;

        StartCoroutine(GenerateMovementPatterns());
        //wait until sequence is complete before continuing
        while (!movementPatternGenerationComplete)
        {
            yield return null;
        }

        GenerateMovementPatternSequence();

        yield return null;

        GenerateBossStats();

        yield return null;
        
        GameManager.Instance.StartAttackSequence();

        UIManager.Instance.SetGeneratingInProgressLabelEnabled(false);

        yield return null;

        generationInProgress = false;
    }

    /// <summary>
    /// Assigns the value of the Boss Type from the UI, and randomises it if set to "Random"
    /// </summary>
    private void SetBossType()
    {
        BossTypeName typeName;
        typeName = UIManager.Instance.GetBossTypeName();

        if (typeName == BossTypeName.Random)
        {
            int index = rand.Next(0, System.Enum.GetNames(typeof(BossTypeName)).Length - 1) + 1; //-1 and +1 to avoid setting boss type to RANDOM again
            typeName = (BossTypeName)index;
            UIManager.Instance.ShowRandomBossType(typeName.ToString());
        }

        bossType = System.Array.Find<BossType>(bossTypeVariables, BossTypeVariables => BossTypeVariables.typeName == typeName);
    }

    /// <summary>
    /// Randomly assigns a new symmetry score
    /// </summary>
    private void GenerateRandomSymmetryScore()
    {
        symmetryValue = rand.Next(0, maxProbabilityValue);
        //Debug.Log("Symmetric seed (0, " + symmetryMax + "): " + symmetryValue);
    }

    /// <summary>
    /// Randomly generates the color palette that will be used for the boss
    /// </summary>
    private void GenerateColorPalette()
    {
        colorQuantity = rand.Next(2, 5);
        colorPalette = new Color[colorQuantity + 1]; //plus 1 for weapon color

        float hue = rand.Next(0, 100) / 100.0f;
        float saturation = rand.Next(0, 100) / 100.0f;
        float brightness = rand.Next(0, 100) / 100.0f;

        colorPalette[0] = Color.HSVToRGB(hue, saturation, brightness);

        //weapon color is final index of color palette and a darker version of the first color
        float weaponBrightness = brightness;
        weaponBrightness -= (weaponBrightness > weaponBrightnessTintAmount) ? weaponBrightnessTintAmount : -weaponBrightnessTintAmount;
        colorPalette[colorQuantity] = Color.HSVToRGB(hue, saturation, weaponBrightness);

        GenerateRandomSymmetryScore();
        bool useComplementaryColors = symmetryValue / (float)maxProbabilityValue > nonComplementaryColorChance;
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

            if (mat != null)
            {
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
    }

    /// <summary>
    /// Takes in the current boss sprite texture generated and applies the color scheme to it
    /// </summary>
    /// <param name="bossTexture"></param>
    private void PaintBossSpriteColor(Texture2D bossTexture)
    {
        float scaleX = rand.Next(perlinColorScaleMin, perlinColorScaleMax);
        float scaleY = rand.Next(perlinColorScaleMin, perlinColorScaleMax);
        float originX = rand.Next(-perlinOriginPointResolution, perlinOriginPointResolution);
        float originY = rand.Next(-perlinOriginPointResolution, perlinOriginPointResolution);

        bool symmetricColor = false;
        GenerateRandomSymmetryScore();
        if (symmetryValue / (float)maxProbabilityValue > asymmetricPatternChance)
        {
            symmetricColor = true;
        }

        float noiseColorBoundaryWidth = 1 / (float)colorQuantity;
        for (int y = 0; y < bossTexture.height; y++)
        {
            for (int x = 0; x < bossTexture.width; x++)
            {
                if (bossTexture.GetPixel(x, y).a != 0.0f)
                {
                    float xCoord = x;
                    float yCoord = y;

                    if (symmetricColor)
                    {
                        xCoord = Mathf.Abs(x - bossTexture.width / 2.0f);
                    }

                    float noise = Mathf.PerlinNoise((xCoord / scaleX) + originX, (yCoord / scaleY) + originY);

                    bossTexture.SetPixel(x, y, colorPalette[(int)(noise / noiseColorBoundaryWidth)]);
                }
            }
        }

        bossTexture.Apply();
    }

    /// <summary>
    /// takes in a weapon texture, applies multiply blend of weapon color to a copy and returns it
    /// </summary>
    /// <param name="inWeaponTexture">the weapon texture to copy</param>
    /// <returns>the painted copy of the texture</returns>
    private Texture2D PaintWeaponTextureColor(Texture2D inWeaponTexture)
    {
        Texture2D outTexture = new Texture2D(inWeaponTexture.width, inWeaponTexture.height);

        TextureDraw.Clear(outTexture);

        for (int y = 0; y < inWeaponTexture.height; y++)
        {
            for (int x = 0; x < inWeaponTexture.width; x++)
            {
                Color pixelColor = inWeaponTexture.GetPixel(x, y);

                if (pixelColor.a == 1.0f)
                {
                    outTexture.SetPixel(x, y, TextureDraw.MultiplyBlendPixel(pixelColor, colorPalette[colorQuantity]));
                }
            }
        }

        outTexture.Apply();

        return outTexture;
    }

    /// <summary>
    /// Generates the sprite that makes up the body of the boss
    /// </summary>
    private IEnumerator GenerateSprite()
    {
        if (spriteSnapshotCam && snapshotSpriteObj && spriteGenerationShapes.Length > 0)
        {
            bossSprite.enabled = true;

            SpriteRenderer snapshotSprite = snapshotSpriteObj.GetComponentInChildren<SpriteRenderer>();

            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            //Clear texture
            TextureDraw.Clear(texture);

            //make sure there is a complexity curve
            if (bossType.spriteComplexityCurve.length == 0)
            {
                Debug.Log("ERROR: Missing complexity curve");
                StopBossGeneration();
                yield break;
            }

            //generate sprite complexity
            spriteShapeComplexity = Mathf.RoundToInt(bossType.spriteComplexityCurve.Evaluate(rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue));
            //Debug.Log("Sprite Shape Complexity: " + spriteShapeComplexity);
            //make sure there is at least one shape
            spriteShapeComplexity = Mathf.Max(spriteShapeComplexity, 1);

            for (int i = 0; i < spriteShapeComplexity; i++)
            {
                GenerateRandomSymmetryScore(); //symmetry score must be calculated again for every shape so they don't all do the same thing
                
                snapshotSpriteObj.transform.rotation = Quaternion.identity;

                ShapeType spriteShape;
                int index = rand.Next(0, spriteGenerationShapes.Length);
                spriteShape = spriteGenerationShapes[index];

                snapshotSprite.sprite = spriteShape.sprite;

                //set width of shape
                int minWidth = (int)(maxBossWidth / minShapeFractionOfMax);
                int widthValue = rand.Next(minWidth, (int)(maxBossWidth * shapeSizeLimiter));
                float objWidth = widthValue / (float)maxBossWidth;

                int heightValue;
                float objHeight;
                bool generateHeight = spriteShape.twoDimensionSizeGeneration;
                //if the shape is scaled in two dimensions instead of just one, set height
                if (generateHeight)
                {
                    int minHeight = (int)(maxBossHeight / minShapeFractionOfMax);
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
                if (symmetryValue >= asymmetricMax * maxProbabilityValue && symmetryValue < normaliseRotMax * maxProbabilityValue && spriteShape.generateRotation)
                {
                    //normalise rotation
                    rotValue = (int)(Mathf.Round(rotValue / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot * Mathf.Sign(rotValue));
                }
                else if (symmetryValue >= normaliseRotMax * maxProbabilityValue && symmetryValue < centreXMax * maxProbabilityValue)
                {
                    //normalise rotation
                    rotValue = spriteShape.generateRotation ? (int)(Mathf.Round(rotValue / spriteShape.nearestSymmetricalRot) * spriteShape.nearestSymmetricalRot * Mathf.Sign(rotValue)) : 0;
                    //centre shape in x axis
                    xValue = textureWidth / 2;
                }
                else if (symmetryValue >= centreXMax * maxProbabilityValue && symmetryValue < mirrorMax * maxProbabilityValue)
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

                texture.Apply();

                bossSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                yield return generationStepDelayTime;
            }

            //paint boss texture
            PaintBossSpriteColor(texture);
            bossSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            yield return generationStepDelayTime;

            //add black outline to sprite texture
            Texture2D outlineTexture = texture;
            outlineTexture = TextureDraw.OutlineTexture(texture, Color.black, bossSpriteOutlineWidth);
            bossSprite.sprite = Sprite.Create(outlineTexture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            yield return generationStepDelayTime;

            //add white outline to sprite texture
            outlineTexture = TextureDraw.OutlineTexture(outlineTexture, Color.white, bossSpriteOutlineWidth);
            bossSprite.sprite = Sprite.Create(outlineTexture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            yield return generationStepDelayTime;

            spriteGenerationComplete = true;
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
    /// Goes through all weapons attached to boss gameobject and removes them
    /// </summary>
    private void ClearWeapons()
    {
        weaponManager.DisableAllWeapons();

        bossWeapons.Clear();
    }

    private bool GenerateWeaponType(ref WeaponType weaponType)
    {
        bool weaponPicked = false;
        int count = 0;
        int weaponTypeIndex;
        int bossTypeMask = 1 << (int)bossType.typeName;

        do
        {
            weaponTypeIndex = rand.Next(0, generatableWeapons.Length);
            weaponType = generatableWeapons[weaponTypeIndex];

            count++;
            if (((int)weaponType.bossTypesWeaponWieldableBy & bossTypeMask) == bossTypeMask)
            {
                weaponPicked = true;
            }
            else if (count > maxWeaponTypeAttempts)
            {
                Debug.Log("Couldn't pick weapon for this boss type");
                break;
            }
        } while (!weaponPicked);

        return weaponPicked;
    }

    private bool GenerateWeaponOrientationMode(WeaponType weaponType, ref WeaponOrientationMode orientationMode)
    {
        bool orientationPicked = false;
        int orientationIndex;
        int orientationMask;
        int count = 0;

        do
        {
            orientationIndex = rand.Next(0, System.Enum.GetNames(typeof(WeaponOrientationMode)).Length);
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
                if (symmetryValue >= centreXMin * maxProbabilityValue && symmetryValue < centreXMax * maxProbabilityValue)
                {
                    if (orientationMode != WeaponOrientationMode.FIXEDSIDEWAYS &&
                        orientationMode != WeaponOrientationMode.FIXEDOTHERFORWARDS &&
                        orientationMode != WeaponOrientationMode.FIXEDOTHER)
                    {
                        orientationPicked = true;
                    }
                }
                else
                {
                    orientationPicked = true;
                }
            }
            else if (count > maxWeaponOrientationAttempts)
            {
                Debug.Log("Couldn't pick orientation for this weapon");
                break;
            }
        } while (!orientationPicked);

        return orientationPicked;
    }

    private IEnumerator GenerateWeaponPosition(WeaponType weaponType, Transform weaponTransform, Transform mirrorWeaponTransform = null)
    {
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        //bool for whether weapon is placed on sprite correctly
        //if weapon can float ignore process and just pick first
        weaponPositionFound = weaponType.canWeaponFloat;

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
                weaponPositionFound = false;
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
            if (symmetryValue >= centreXMin * maxProbabilityValue && symmetryValue < centreXMax * maxProbabilityValue)
            {
                xSeed = 0;
            }
            else
            {
                xSeed = rand.Next((int)(-weaponXLimit * 100), (int)(weaponXLimit * 100));
            }

            ySeed = rand.Next((int)(-weaponYLimit * 100), (int)(weaponYLimit * 100));

            weaponTransform.position = new Vector3(xSeed / 100.0f, ySeed / 100.0f, 0.0f) + bossObj.transform.position;

            //if a raycast does not hit the boss sprite collider from this weapon position, try again
            if (!Physics2D.Raycast(weaponTransform.position, weaponTransform.forward, 1000, bossSpriteLayer) && !weaponPositionFound)
            {
                continue;
            }

            yield return waitFrame;

            if (weaponTransform.GetComponent<Weapon>().GetCollidingWithOtherWeapon())
            {
                continue;
            }

            yield return generationStepDelayTime;

            //if weapon uses mirror symmetry, do the same check for mirror weapon
            if (mirrorWeaponTransform != null)
            {
                mirrorXSeed = -xSeed;

                mirrorWeaponTransform.position = new Vector3(mirrorXSeed / 100.0f, ySeed / 100.0f, 0.0f) + bossObj.transform.position;

                if (!Physics2D.Raycast(mirrorWeaponTransform.position, mirrorWeaponTransform.forward, 1000, bossSpriteLayer) && !weaponPositionFound)
                {
                    continue;
                }

                yield return waitFrame;

                if (mirrorWeaponTransform.GetComponent<Weapon>().GetCollidingWithOtherWeapon())
                {
                    continue;
                }           
            }

            weaponPositionFound = true;
        } while (!weaponPositionFound);

        weaponPositioningProcessComplete = true;
    }

    /// <summary>
    /// Randomly select the weapons that will be attached to the boss
    /// </summary>
    private IEnumerator GenerateWeapons()
    {
        //make sure there is a complexity curve
        if (bossType.weaponQuantityCurve.length == 0)
        {
            Debug.Log("ERROR: Missing weapon quantity curve");
            weaponGenerationComplete = true;
            yield break;
        }

        //generate number of weapons
        weaponQuantity = Mathf.RoundToInt(bossType.weaponQuantityCurve.Evaluate(rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue));
        //Debug.Log("Number of weapons: " + weaponQuantity);

        for (int i = 0; i < weaponQuantity; i++)
        {
            GenerateRandomSymmetryScore(); //symmetry score must be calculated again for each new weapon so they don't all do the same thing.

            //pick random weapon type
            WeaponType weaponType = new WeaponType();
            bool typeFound = GenerateWeaponType(ref weaponType);

            //skip this weapon if a weapon can't be found
            if (!typeFound)
            {
                continue;
            }

            //create weapon gameobject
            GameObject weaponObj = weaponManager.GetWeaponObject();
            weaponObj.SetActive(true);
            Weapon weapon = weaponObj.GetComponent<Weapon>();            

            //set up weapon sprite
            SpriteRenderer sr = weaponObj.GetComponentInChildren<SpriteRenderer>();
            sr.sprite = weaponType.sprite;
            //set weapon size
            weaponObj.transform.localScale = new Vector3(weaponType.size, weaponType.size);
            //offset sprite to pivot
            weapon.sprite.localPosition = weaponType.spritePivotPointOffset;
            //set offset for where attack will spawn from
            weapon.attackSource.localPosition = weaponType.attackSourcePositionOffset;
            //add collider
            sr.gameObject.AddComponent<PolygonCollider2D>();
            sr.gameObject.GetComponent<PolygonCollider2D>().isTrigger = true;

            //set weapon orientation mode
            WeaponOrientationMode orientationMode = new WeaponOrientationMode();
            bool orientationModeFound = GenerateWeaponOrientationMode(weaponType, ref orientationMode);

            //if setting orientation mode failed, destroy weapon and move on
            if (!orientationModeFound)
            {
                weaponObj.SetActive(false);
                continue;
            }
            //set up orientation mode of weapon
            weapon.CurrentOrientationMode = orientationMode;

            //check symmetry score and if weapon should use mirror symmetry instantiate new weapon object
            GameObject mirrorWeaponObj = null;
            Weapon mirrorWeapon = null;

            //calculate probability bounds for weapon being mirrored symmetrically
            float asymmetric = weaponType.AsymmetricProbability * bossType.asymmetricProbabilityMultiplier;
            float centreX = weaponType.CentreXProbability * bossType.centreXProbabilityMultiplier;
            float mirror = weaponType.MirrorProbability * bossType.mirrorProbabilityMultiplier;
            float symmetryProbabilitiesTotal = asymmetric + centreX + mirror;
            float mirrorSymmetryMin = (asymmetric + centreX) / symmetryProbabilitiesTotal;
            float mirrorSymmetryMax = 1.0f;

            SpriteRenderer mirrorsr = null;
            bool mirroringWeapon = false;

            if (symmetryValue >= mirrorSymmetryMin * maxProbabilityValue && symmetryValue < mirrorSymmetryMax * maxProbabilityValue)
            {
                mirroringWeapon = true;
                mirrorWeaponObj = weaponManager.GetWeaponObject();
                mirrorWeaponObj.SetActive(true);
                mirrorWeapon = mirrorWeaponObj.GetComponent<Weapon>();

                //set up weapons as mirrored pairs
                weapon.mirrorPair = mirrorWeapon;
                mirrorWeapon.mirrorPair = weapon;

                //set up weapon sprite
                mirrorsr = mirrorWeaponObj.GetComponentInChildren<SpriteRenderer>();
                mirrorsr.sprite = weaponType.sprite;
                //set weapon size
                mirrorWeaponObj.transform.localScale = new Vector3(weaponType.size, weaponType.size);
                //offset sprite to pivot
                mirrorWeapon.sprite.localPosition = weaponType.spritePivotPointOffset;
                //set offset for where attack will spawn from
                mirrorWeapon.attackSource.localPosition = weaponType.attackSourcePositionOffset;
                //add collider
                mirrorsr.gameObject.AddComponent<PolygonCollider2D>();
                mirrorsr.gameObject.GetComponent<PolygonCollider2D>().isTrigger = true;
                //set weapon orientation mode
                mirrorWeapon.CurrentOrientationMode = orientationMode;
            }

            weaponPositioningProcessComplete = false;
            weaponPositionFound = false;
            //position weapon on boss sprite 
            if (!mirroringWeapon)
            {
                StartCoroutine(GenerateWeaponPosition(weaponType, weaponObj.transform));
            }
            else
            {
                StartCoroutine(GenerateWeaponPosition(weaponType, weaponObj.transform, mirrorWeaponObj.transform));
            }

            while (!weaponPositioningProcessComplete)
            {
                yield return null;
            }

            //if weapon could not find a position, destroy weapons
            if (!weaponPositionFound)
            {
                weaponObj.SetActive(false);
                if (mirrorWeaponObj != null)
                {
                    mirrorWeaponObj.SetActive(false);
                }
                continue;
            }

            //appearance step
            Sprite weaponSprite = sr.sprite;
            Texture2D weaponTexture = weaponSprite.texture;

            //color weapon sprite using generated color palette if sprite is to be recoloured
            if (weaponType.generateWeaponColor == true)
            {
                weaponTexture = PaintWeaponTextureColor(weaponTexture);

                weaponSprite = Sprite.Create(weaponTexture, new Rect(0, 0, weaponTexture.width, weaponTexture.height), new Vector2(0.5f, 0.5f));
                sr.sprite = weaponSprite;

                yield return generationStepDelayTime;
            }

            //add texture outlines
            weaponTexture = TextureDraw.OutlineTexture(weaponTexture, Color.black, weaponSpriteOutlineWidth);
            weaponSprite = Sprite.Create(weaponTexture, new Rect(0, 0, weaponTexture.width, weaponTexture.height), new Vector2(0.5f, 0.5f));
            sr.sprite = weaponSprite;

            yield return generationStepDelayTime;

            weaponTexture = TextureDraw.OutlineTexture(weaponTexture, Color.white, weaponSpriteOutlineWidth);
            weaponSprite = Sprite.Create(weaponTexture, new Rect(0, 0, weaponTexture.width, weaponTexture.height), new Vector2(0.5f, 0.5f));
            sr.sprite = weaponSprite;

            yield return generationStepDelayTime;

            //color mirrored weapon sprite
            if (mirroringWeapon)
            {
                mirrorsr.sprite = weaponSprite;
            }

            //set weapon rotation according to orientationMode
            switch (orientationMode)
            {
                case WeaponOrientationMode.FIXEDFORWARD:
                case WeaponOrientationMode.NONORIENTED:
                    weapon.SetWeaponRotation(0);
                    break;
                case WeaponOrientationMode.ROTATABLE:
                    weapon.SetWeaponRotation(0);
                    weapon.SetRotatable(false);

                    if (mirroringWeapon)
                    {
                        mirrorWeapon.SetRotatable(false);
                    }
                    break;
                case WeaponOrientationMode.TRACKSPLAYER:
                    weapon.SetWeaponRotation(0);
                    weapon.SetRotatable(true);

                    if (mirroringWeapon)
                    {
                        mirrorWeapon.SetRotatable(true);
                    }
                    break;
                case WeaponOrientationMode.FIXEDSIDEWAYS:
                    {
                        weapon.SetWeaponRotation(90 * Mathf.Sign(weaponObj.transform.position.x));

                        if (mirroringWeapon)
                        {
                            mirrorWeapon.SetWeaponRotation(90 * Mathf.Sign(mirrorWeaponObj.transform.position.x));
                        }
                    }
                    break;
                case WeaponOrientationMode.FIXEDOTHERFORWARDS:
                    {
                        int randomOrientation = rand.Next(0, 90);
                        weapon.SetWeaponRotation(randomOrientation * Mathf.Sign(weaponObj.transform.position.x));

                        if (mirroringWeapon)
                        {
                            mirrorWeapon.SetWeaponRotation(randomOrientation * Mathf.Sign(mirrorWeaponObj.transform.position.x));
                        }
                    }
                    break;
                case WeaponOrientationMode.FIXEDOTHER:
                    {
                        int randomOrientation = rand.Next(0, 180);
                        weapon.SetWeaponRotation(randomOrientation * Mathf.Sign(weaponObj.transform.position.x));

                        if (mirroringWeapon)
                        {
                            mirrorWeapon.SetWeaponRotation(randomOrientation * Mathf.Sign(mirrorWeaponObj.transform.position.x));
                        }
                    }
                    break;
            }

            bossWeapons.Add(weapon);
            if (mirroringWeapon)
            {
                bossWeapons.Add(mirrorWeapon);
            }

            yield return generationStepDelayTime;
        }

        weaponGenerationComplete = true;
    }

    private void ClearAttacks()
    {
        bossAttackTypes.Clear();
        bossAttackSequence.Clear();
    }

    private bool GenerateAttackType(ref IAttackType attackType)
    {
        bool attackPicked = false;
        int count = 0;
        int attackTypeIndex;
        int bossTypeMask = 1 << (int)bossType.typeName;

        do
        {
            attackTypeIndex = rand.Next(0, generatableAttackTypes.Length);
            attackType = (IAttackType)Instantiate(generatableAttackTypes[attackTypeIndex]);

            if (attackType == null)
            {
                continue;
            }

            count++;

            if (((int)attackType.GetCompatibleBossTypes() & bossTypeMask) == bossTypeMask)
            {
                attackPicked = true;
            }
            else if (count > maxWeaponTypeAttempts)
            {
                Debug.Log("Couldn't pick attack for this boss");
                break;
            }
        } while (!attackPicked);

        return attackPicked;
    }

    private bool LocateWeaponForAttack(IAttackType attack, out Weapon weapon)
    {
        bool weaponLocated = false;
        int count = 0;
        int weaponIndex;
        int weaponOrientationMask;
        
        do
        {
            weaponIndex = rand.Next(0, bossWeapons.Count);

            weapon = bossWeapons[weaponIndex];

            if (weapon == null)
            {
                continue;
            }

            weaponOrientationMask = 1 << (int)weapon.CurrentOrientationMode;

            count++;
            if (((int)attack.GetRequiredWeaponTypes() & weaponOrientationMask) == weaponOrientationMask)
            {
                weaponLocated = true;
            }
            else if (count > maxWeaponTypeAttempts)
            {
                Debug.Log("Couldn't locate weapon for this attack type");
                break;
            }
        } while (!weaponLocated);

        return weaponLocated;
    }

    private IEnumerator GenerateAttacks()
    {
        //TODO tie into boss difficulty
        //minimum quantity is largest of number of weapons or 1
        attackQuantity = rand.Next(Mathf.Max(bossWeapons.Count, 1), attackQuantityMax);
        //Debug.Log("Attack Quantity: " + attackQuantity);

        int iterations = attackQuantity;

        for (int i = 0; i < iterations; i++)
        {
            IAttackType attackType = null;
            bool attackTypeFound = GenerateAttackType(ref attackType);

            if (!attackTypeFound)
            {
                attackQuantity--;
                continue;
            }

            //assign weapon to the attack
            Weapon attackWeapon;
            //if the attack uses a weapon
            if (attackType.GetRequiredWeaponTypes() != 0)
            {
                if (bossWeapons.Count > 0)
                {
                    bool foundWeapon = LocateWeaponForAttack(attackType, out attackWeapon);

                    if (!foundWeapon)
                    {
                        continue;
                    }

                    attackType.SetupAttack(attackWeapon.gameObject);
                }
            }
            else
            {
                attackType.SetupAttack();
            }

            bossAttackTypes.Add(attackType);

            yield return generationStepDelayTime;
        }

        attackGenerationComplete = true;
    }

    private void GenerateAttackSequence()
    {
        if (bossAttackTypes.Count > 0)
        {
            //minimum sequence length is highest of set minimum and attack quantity
            int attackSequenceLength = rand.Next(Mathf.Max(attackQuantity, attackSequenceLengthMin), attackSequenceLengthMax);
            
            for (int i = 0; i < attackSequenceLength; i++)
            {
                int attack = rand.Next(0, bossAttackTypes.Count);
                bossAttackSequence.Add(bossAttackTypes[attack]);
            }

            bossLogic.SetupAttackSequence(bossAttackSequence);
        }
    }

    private void ClearMovementPatterns()
    {
        generatedMovementPatterns.Clear();
        bossMovementPatternSequence.Clear();
    }

    private IEnumerator GenerateMovementPatterns()
    {
        movementPatternQuantity = rand.Next(movementPatternQuantityMin, movementPatternQuantityMax);
        
        for (int i = 0; i < movementPatternQuantity; i++)
        {
            MovementPatternType movementPattern = ScriptableObject.CreateInstance<MovementPatternType>(); ;

            //make sure there is a complexity curve
            if (movementsPerPatternProbabilityWeighting.length == 0)
            {
                Debug.Log("ERROR: Missing movements per pattern curve");
                movementPatternGenerationComplete = true;
                yield break;
            }
            
            int patternMovementQuantity = Mathf.RoundToInt(movementsPerPatternProbabilityWeighting.Evaluate(rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue));
            movementPattern.numberOfMovements = patternMovementQuantity;

            int destinationCount = rand.Next(destinationPointCountMin, destinationPointCountMax);
            Vector3[] points = new Vector3[destinationCount];
            for (int j = 0; j < destinationCount; j++)
            {
                float x = rand.Next((int)(minDestinationX * 100), (int)(maxDestinationX * 100));
                float y = rand.Next((int)(minDestinationY * 100), (int)(maxDestinationY * 100));
                points[j] = new Vector3(x / 100.0f, y / 100.0f, 0.0f);
            }
            movementPattern.SetDestinationPoints(points);
            
            if (rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue < includeStartPointChance)
            {
                movementPattern.includeStartPointInDestinations = true;
            }

            bool constrainX = false;
            bool constrainY = false;
            if (rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue < constrainXAxisChance)
            {
                constrainX = true;
            }
            else if (rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue < constrainYAxisChance)
            {
                constrainY = true;
            }
            movementPattern.SetAxisConstraints(constrainX, constrainY);
            
            if (rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue < randomlyDecideNextDestinationChance)
            {
                movementPattern.SetRandomlyDecideNextDestination(true);
            }

            float destinationWaitTime = destinationWaitTimeProbabilityWeighting.Evaluate(rand.Next(0, maxProbabilityValue) / (float)maxProbabilityValue);
            movementPattern.waitTimeAtDestination = destinationWaitTime;

            int accelerationIndex = rand.Next(0, availableAccelerationTypes.Count);
            movementPattern.accelerationType = availableAccelerationTypes[accelerationIndex];

            generatedMovementPatterns.Add(movementPattern);
        }

        movementPatternGenerationComplete = true;

        yield return null; 
    }

    private void GenerateMovementPatternSequence()
    {
        if (generatedMovementPatterns.Count > 0)
        {
            //minimum sequence length is highest of set minimum and attack quantity
            int movementPatternSequenceLength = rand.Next(Mathf.Max(movementPatternQuantity, movementPatternSequenceLengthMin), movementPatternSequenceLengthMax);

            for (int i = 0; i < movementPatternSequenceLength; i++)
            {
                int pattern = rand.Next(0, generatedMovementPatterns.Count);
                bossMovementPatternSequence.Add(generatedMovementPatterns[pattern]);
            }

            UIManager.Instance.SetMovementPatternSequenceSize(generatedMovementPatterns.Count);
            bossLogic.SetupMovementPatternSequence(generatedMovementPatterns);
        }
    }

    private void GenerateBossStats()
    {
        int bossLife = rand.Next(minBossLifePoints, maxBossLifePoints);
        GameManager.Instance.SetBossLife(bossLife);
        UIManager.Instance.SetMaxLifePoints(bossLife);
        UIManager.Instance.SetBossLifebarActive(true);

        float bossSpeedModifier = rand.Next((int)(minBossSpeed * 100), (int)(maxBossSpeed * 100)) / 100.0f; //scale up and down by 100 to work around int generation restrictions
        GameManager.Instance.SetBossSpeed(bossSpeedModifier);
        UIManager.Instance.SetSpeedFactor(bossSpeedModifier);
    }
}