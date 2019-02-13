using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    public static GeneratorScript Instance;

    private GameObject bossObj;

    private Camera spriteSnapshotCam;

    private GameObject snapshotSpriteObj;

    [SerializeField]
    private int maxBossWidth = 500, maxBossHeight = 500;
    private int textureWidth, textureHeight;
    private int xOffset, yOffset;

    //Random values
    [SerializeField]
    private int shapeMax = 20, symmetricMax = 4;

    [SerializeField]
    private int spriteShapeComplexity = 3;

    [SerializeField]
    private Sprite[] ComponentShapeSprites;

    private enum Shape
    {
        CIRCLE,
        RING,
        ELLIPSE,
        SEMICIRCLE,
        SQUARE,
        RECTANGLE,
        DIAMOND,
        RHOMBUS,
        EQUITRI,
        ISOTRI,
        RIGHTANGLETRI,
        SCALENETRI,
        PENTAGON,
        HEXAGON
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        bossObj = GameObject.FindWithTag("Boss");

        spriteSnapshotCam = GameObject.FindWithTag("SpriteSnapshotCam").GetComponent<Camera>();

        snapshotSpriteObj = GameObject.FindWithTag("SnapshotSpriteObj");

        textureWidth = (int)(maxBossWidth * 1.25f);
        textureHeight = (int)(maxBossHeight * 1.25f);
        xOffset = (textureWidth - maxBossWidth) / 2;
        yOffset = (textureHeight - maxBossHeight) / 2;
    }

    public void GenerateBoss()
    {
        if (bossObj && spriteSnapshotCam && snapshotSpriteObj)
        {
            SpriteRenderer sprite = bossObj.GetComponentInChildren<SpriteRenderer>();

            SpriteRenderer snapshotSprite = snapshotSpriteObj.GetComponentInChildren<SpriteRenderer>();

            if (sprite)
            {
                Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                texture.wrapMode = TextureWrapMode.Clamp;

                //TextureDraw.Clear(texture);
                for (int y = yOffset; y < maxBossHeight + yOffset; y++)
                {
                    for (int x = xOffset; x < maxBossWidth + xOffset; x++)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }

                for (int i = 0; i < spriteShapeComplexity; i++)
                {
                    int shapeSeed = Random.Range(0, shapeMax);
                    int symmetricSeed = Random.Range(0, symmetricMax);
                    Debug.Log("Shape seed (0, " + shapeMax + "): " + shapeSeed);
                    Debug.Log("Symmetric seed (0, " + symmetricMax + "): " + symmetricSeed);

                    Shape spriteShape = Shape.CIRCLE;

                    if (shapeSeed < shapeMax / 2)
                    {
                        spriteShape = Shape.CIRCLE;
                    }
                    else
                    {
                        spriteShape = Shape.SQUARE;
                    }

                    int x0 = (textureWidth / 2);
                    int y0 = (textureHeight / 2);

                    if (spriteShape == Shape.SQUARE)
                    {
                        int index = System.Array.FindIndex(ComponentShapeSprites, s => s.name == "Rect");
                        snapshotSprite.sprite = ComponentShapeSprites[index];

                        int minWidth = maxBossWidth / 10;
                        int minHeight = maxBossHeight / 10;

                        int widthSeed = Random.Range(minWidth, maxBossWidth);
                        int heightSeed = Random.Range(minHeight, maxBossHeight);
                        float width = widthSeed / (float)maxBossWidth;
                        float height = heightSeed / (float)maxBossHeight;
                        Debug.Log("Width seed (" + minWidth + ", " + maxBossWidth + "): " + widthSeed + " (" + width + "%)");
                        Debug.Log("Height seed (" + minHeight + ", " + maxBossHeight + "): " + heightSeed + " (" + height + "%)");

                        snapshotSpriteObj.transform.localScale = new Vector3(width, height);
                        
                        int xMax = maxBossWidth + xOffset - (widthSeed / 2);
                        int xMin = xOffset + (widthSeed / 2);
                        int xSeed = Random.Range(xMin, xMax);
                        Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);

                        x0 = xSeed;

                        int yMax = maxBossHeight + yOffset - (heightSeed / 2);
                        int yMin = yOffset + (heightSeed / 2);
                        int ySeed = Random.Range(yMin, yMax);
                        Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);
                        
                        y0 = ySeed;
                    }
                    else if (spriteShape == Shape.CIRCLE)
                    {
                        int index = System.Array.FindIndex(ComponentShapeSprites, s => s.name == "Ellipse");
                        snapshotSprite.sprite = ComponentShapeSprites[index];

                        int radiusMax = maxBossWidth / 2;
                        int radiusMin = radiusMax / 10;
                        int radiusSeed = Random.Range(radiusMin, radiusMax);
                        float scale = radiusSeed / (float)radiusMax;
                        Debug.Log("Radius seed (" + radiusMin + ", " + radiusMax + "): " + radiusSeed + " (" + scale + "%)");
                        
                        snapshotSpriteObj.transform.localScale = new Vector3(scale, scale);

                        int xMax = maxBossWidth + xOffset - radiusSeed;
                        int xMin = xOffset + radiusSeed;
                        int xSeed = Random.Range(xMin, xMax);
                        Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);

                        x0 = xSeed;

                        int yMax = maxBossHeight + yOffset - radiusSeed;
                        int yMin = yOffset + radiusSeed;
                        int ySeed = Random.Range(yMin, yMax);
                        Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);

                        y0 = ySeed;

                    /*
                        if (symmetricSeed >= symmetricMax * 0.2)
                        {
                            if (symmetricSeed > symmetricMax * 0.6)
                            {
                                //if symmetric type 1, double up shape on both sides
                                //get opposite xSeed
                                int x2 = 2 * (textureWidth / 2) - xSeed;
                                TextureDraw.Circle(texture, x2, ySeed, radiusSeed, Color.red);
                            }
                            else
                            {
                                //if symmetric type 2, simply put shape in the middle
                                xSeed = textureWidth / 2;
                            }
                        }*/
                    }

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

                texture.Apply();

                sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                sprite.enabled = true;
            }
        }
    }
}
