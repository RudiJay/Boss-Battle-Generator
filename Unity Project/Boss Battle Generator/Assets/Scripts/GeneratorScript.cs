using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShapeType
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
    HEX
}

[System.Serializable]
public struct SpriteShape
{
    public ShapeType shape;

    public Sprite sprite;

    [Range(0,1)]
    public float[] symmetryProbBounds;
}

public class GeneratorScript : MonoBehaviour
{
    public static GeneratorScript Instance;

    private System.Random rand;

    private GameObject bossObj;

    private Camera spriteSnapshotCam;

    private GameObject snapshotSpriteObj;

    private InputField seedInputField;

    private IEnumerator autoGenerator;
    private bool isAutoGenerating = false;

    [Header("Interface")]
    [SerializeField]
    private float autoGeneratorDelay = 2.0f;
    [SerializeField]
    private bool ShowSpriteBorder = false;

    
    [Header("Generation Variables")]
    [SerializeField]
    private int seed;

    [SerializeField]
    private int maxBossWidth = 500, maxBossHeight = 500;
    private int textureWidth, textureHeight;
    private int xOffset, yOffset;
    [SerializeField][Range(0,1)]
    private float shapeSizeLimiter = 0.75f;

    //Random values
    [SerializeField]
    private int shapeMax = 20, symmetricMax = 4;

    [SerializeField]
    private int spriteShapeComplexity = 3;

    [SerializeField][Space(20)]
    private SpriteShape[] SpriteGenerationShapes;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rand = new System.Random();

        autoGenerator = AutoGenerateBossFights();

        bossObj = GameObject.FindWithTag("Boss");

        spriteSnapshotCam = GameObject.FindWithTag("SpriteSnapshotCam").GetComponent<Camera>();

        snapshotSpriteObj = GameObject.FindWithTag("SnapshotSpriteObj");

        seedInputField = GameObject.FindWithTag("SeedInputField").GetComponent<InputField>();

        textureWidth = (int)(maxBossWidth * 1.25f);
        textureHeight = (int)(maxBossHeight * 1.25f);
        xOffset = (textureWidth - maxBossWidth) / 2;
        yOffset = (textureHeight - maxBossHeight) / 2;
    }

    private void Update()
    {
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

    private IEnumerator AutoGenerateBossFights()
    {
        while (true)
        {
            WaitForSeconds delay = new WaitForSeconds(autoGeneratorDelay);

            GenerateBossFight(true);

            yield return delay;
        }
    }

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

    public void GenerateSeed()
    {
        seed = System.Environment.TickCount;

        seedInputField.text = seed.ToString();

        rand = new System.Random(seed);
    }

    public void SetSeed(string inString)
    {
        if (inString.Length > 0 && inString.Length <= 9)
        {
            seed = int.Parse(inString);
        }

        rand = new System.Random(seed);

        GenerateBossFight(false);
    }

    public void GenerateBossFight(bool seedNeedsSetting)
    {
        if (seedNeedsSetting)
        {
            GenerateSeed();
        }

        Debug.Log("Boss Fight Seed: " + seed);

        GenerateSprite(false);
    }

    public void GenerateSprite(bool seedNeedsSetting)
    {
        if (seedNeedsSetting)
        {
            GenerateSeed();
        }

        if (bossObj && spriteSnapshotCam && snapshotSpriteObj && SpriteGenerationShapes.Length > 0)
        {
            SpriteRenderer sprite = bossObj.GetComponentInChildren<SpriteRenderer>();

            SpriteRenderer snapshotSprite = snapshotSpriteObj.GetComponentInChildren<SpriteRenderer>();

            if (sprite)
            {
                Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                texture.wrapMode = TextureWrapMode.Clamp;

                //whether to clear out a the space the sprite can occupy, leaving the border around it
                //or just clear out the whole background
                if (ShowSpriteBorder)
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
                    int symmetricSeed = rand.Next(0, symmetricMax);
                    Debug.Log("Shape seed (0, " + shapeMax + "): " + shapeSeed);
                    Debug.Log("Symmetric seed (0, " + symmetricMax + "): " + symmetricSeed);

                    snapshotSpriteObj.transform.rotation = Quaternion.identity;

                    SpriteShape spriteShape;
                    int index = (int)(shapeSeed / (float)shapeMax * SpriteGenerationShapes.Length);
                    //index = 9;
                    spriteShape = SpriteGenerationShapes[index];

                    snapshotSprite.sprite = spriteShape.sprite;

                    int x0 = (textureWidth / 2);
                    int y0 = (textureHeight / 2);

                    switch (spriteShape.shape)
                    {
                        case ShapeType.CIRCLE:
                        case ShapeType.RING:
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
                                    int x2 = 2 * (textureWidth / 2) - xSeed;
                                    DrawShapeFromSnapshot(texture, x2, ySeed);
                                }
                                //else no symmetry

                                x0 = xSeed;
                                y0 = ySeed;

                                Debug.Log("Radius seed (" + radiusMin + ", " + radiusMax + "): " + radiusSeed + " (" + scale + "%)");
                                Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                                Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            };
                            break;
                        case ShapeType.SQUARE:
                        case ShapeType.SEMICIRCLE:
                            {
                                int minSize = maxBossWidth / 10;

                                int rotSeed = rand.Next(-45, 45);

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
                                    //get rid of rotation
                                    rotSeed = (int)(Mathf.Round(rotSeed / 45.0f) * 45 * Mathf.Sign(rotSeed));
                                }
                                else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[1])
                                {
                                    //get rid of rotation
                                    rotSeed = (int)(Mathf.Round(rotSeed / 45.0f) * 45 * Mathf.Sign(rotSeed));
                                    //put shape in the middle
                                    xSeed = textureWidth / 2;
                                }
                                else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[2])
                                {
                                    //double up shape on both sides
                                    //get opposite xSeed
                                    int x2 = 2 * (textureWidth / 2) - xSeed;
                                    //mirror rotation
                                    snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, -rotSeed);
                                    DrawShapeFromSnapshot(texture, x2, ySeed);
                                }
                                //else no symmetry

                                snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, rotSeed);

                                x0 = xSeed;
                                y0 = ySeed;

                                Debug.Log("Rot seed (" + rotSeed + ")");
                                Debug.Log("Size seed (" + minSize + ", " + (int)(maxBossWidth * shapeSizeLimiter) + "): " + sizeSeed + " (" + size + "%)");
                                Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                                Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            }
                            break;
                        case ShapeType.RECT:
                        case ShapeType.OVAL:
                        case ShapeType.HALO:
                        case ShapeType.SEMIOVAL:
                        case ShapeType.DIAMOND:
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

                                snapshotSpriteObj.transform.localScale = new Vector3(width, height);

                                if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[0])
                                {
                                    //get rid of rotation
                                    rotSeed = 0;
                                }
                                else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[1])
                                {
                                    //get rid of rotation
                                    rotSeed = 0;
                                    //put shape in the middle
                                    xSeed = textureWidth / 2;
                                }
                                else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[2])
                                {
                                    //double up shape on both sides
                                    //get opposite xSeed
                                    int x2 = 2 * (textureWidth / 2) - xSeed;
                                    //mirror rotation
                                    snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, -rotSeed);
                                    DrawShapeFromSnapshot(texture, x2, ySeed);
                                }

                                snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, rotSeed);

                                x0 = xSeed;
                                y0 = ySeed;

                                Debug.Log("Rot seed (" + rotSeed + ")");
                                Debug.Log("Width seed (" + minWidth + ", " + (int)(maxBossWidth * shapeSizeLimiter) + "): " + widthSeed + " (" + width + "%)");
                                Debug.Log("Height seed (" + minHeight + ", " + (int)(maxBossHeight * shapeSizeLimiter) + "): " + heightSeed + " (" + height + "%)");
                                Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                                Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            };
                            break;
                        case ShapeType.RHOMBUS:
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
                                    //get rid of rotation
                                    rotSeed = 0;
                                }
                                else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[1])
                                {
                                    //get rid of rotation
                                    rotSeed = 0;
                                    //put shape in the middle
                                    xSeed = textureWidth / 2;
                                }
                                else if (symmetricSeed < symmetricMax * spriteShape.symmetryProbBounds[2])
                                {
                                    //double up shape on both sides
                                    //get opposite xSeed
                                    int x2 = 2 * (textureWidth / 2) - xSeed;
                                    //mirror rotation
                                    snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, -rotSeed);
                                    snapshotSpriteObj.transform.localScale = new Vector3(-width, height);
                                    DrawShapeFromSnapshot(texture, x2, ySeed);
                                }

                                snapshotSpriteObj.transform.rotation = Quaternion.Euler(0, 0, rotSeed);
                                snapshotSpriteObj.transform.localScale = new Vector3(width, height);

                                x0 = xSeed;
                                y0 = ySeed;

                                Debug.Log("Rot seed (" + rotSeed + ")");
                                Debug.Log("Width seed (" + minWidth + ", " + (int)(maxBossWidth * shapeSizeLimiter) + "): " + widthSeed + " (" + width + "%)");
                                Debug.Log("Height seed (" + minHeight + ", " + (int)(maxBossHeight * shapeSizeLimiter) + "): " + heightSeed + " (" + height + "%)");
                                Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);
                                Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                            };
                            break;
                    }

                    DrawShapeFromSnapshot(texture, x0, y0);
                }

                texture.Apply();

                sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                sprite.enabled = true;
            }
        }
    }
}
