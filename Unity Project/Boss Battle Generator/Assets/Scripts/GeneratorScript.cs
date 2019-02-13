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
    }

    public void GenerateBoss()
    {
        if (bossObj && spriteSnapshotCam && snapshotSpriteObj)
        {
            SpriteRenderer sprite = bossObj.GetComponentInChildren<SpriteRenderer>();

            SpriteRenderer snapshotSprite = snapshotSpriteObj.GetComponentInChildren<SpriteRenderer>();

            if (sprite)
            {
                int textureWidth = (int)(maxBossWidth * 1.25f);
                int textureHeight = (int)(maxBossHeight * 1.25f);
                int xOffset = (textureWidth - maxBossWidth) / 2;
                int yOffset = (textureHeight - maxBossHeight) / 2;

                var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
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

                    if (spriteShape == Shape.SQUARE)
                    {
                        int index = System.Array.FindIndex(ComponentShapeSprites, s => s.name == "Rect");
                        snapshotSprite.sprite = ComponentShapeSprites[index];

                        int minWidth = maxBossWidth / 10;
                        int minHeight = maxBossHeight / 10;

                        int widthSeed = Random.Range(minWidth, maxBossWidth);
                        int heightSeed = Random.Range(minHeight, maxBossHeight);
                        Debug.Log("Width seed (" + minWidth + ", " + maxBossWidth + "): " + widthSeed);
                        Debug.Log("Height seed (" + minHeight + ", " + maxBossHeight + "): " + heightSeed);

                        snapshotSpriteObj.transform.localScale = new Vector3(widthSeed / (float)maxBossWidth, heightSeed / (float)maxBossHeight);


                        /*
                            int xMax = maxBossWidth + xOffset - (widthSeed / 2);
                            int xMin = xOffset + (widthSeed / 2);
                            int xSeed = Random.Range(xMin, xMax);
                            Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);

                            int yMax = maxBossHeight + yOffset - (heightSeed / 2);
                            int yMin = yOffset + (heightSeed / 2);
                            int ySeed = Random.Range(yMin, yMax);
                            Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);

                            TextureDraw.Rect(texture, xSeed, ySeed, widthSeed, heightSeed, Color.red);*/
                    }
                    else if (spriteShape == Shape.CIRCLE)
                    {
                        int index = System.Array.FindIndex(ComponentShapeSprites, s => s.name == "Ellipse");
                        snapshotSprite.sprite = ComponentShapeSprites[index];

                        int radiusMax = maxBossWidth / 2;
                        int radiusMin = radiusMax / 10;
                        int radiusSeed = Random.Range(radiusMin, radiusMax);
                        Debug.Log("Radius seed (" + radiusMin + ", " + radiusMax + "): " + radiusSeed);

                        float scale = radiusSeed / (float)radiusMax;
                        snapshotSpriteObj.transform.localScale = new Vector3(scale, scale);

                        /*
                        int xMax = maxBossWidth + xOffset - radiusSeed;
                        int xMin = xOffset + radiusSeed;
                        int xSeed = Random.Range(xMin, xMax);
                        Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);

                        int yMax = maxBossHeight + yOffset - radiusSeed;
                        int yMin = yOffset + radiusSeed;
                        int ySeed = Random.Range(yMin, yMax);
                        Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);

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
                        }

                        TextureDraw.Circle(texture, xSeed, ySeed, radiusSeed, Color.red);*/
                    }

                    spriteSnapshotCam.targetTexture = RenderTexture.GetTemporary(maxBossWidth, maxBossHeight, 16);

                    spriteSnapshotCam.Render();

                    RenderTexture.active = spriteSnapshotCam.targetTexture;
                    float snapWidth = spriteSnapshotCam.targetTexture.width;
                    float snapHeight = spriteSnapshotCam.targetTexture.height;

                    Texture2D snapshot = new Texture2D(textureWidth, textureHeight);
                    snapshot.ReadPixels(new Rect(0, 0, snapWidth, snapHeight), 0, 0);

                    RenderTexture.ReleaseTemporary(spriteSnapshotCam.targetTexture);

                    TextureDraw.CopyFromTexture(texture, snapshot, (textureWidth / 2) - (snapshot.width / 2) + xOffset, (textureHeight / 2) - (snapshot.height / 2) + yOffset);
                }

                texture.Apply();

                sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                sprite.enabled = true;
            }
        }
    }
}
