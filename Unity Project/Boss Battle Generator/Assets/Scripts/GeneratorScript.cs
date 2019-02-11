﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    public static GeneratorScript Instance;

    private GameObject Boss;

    [SerializeField]
    private int maxBossWidth, maxBossHeight;

    //Random values
    [SerializeField]
    int shapeMax = 5, symmetricMax = 4;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Boss = GameObject.FindWithTag("Boss");
    }

    public void GenerateBoss()
    {
        if (Boss)
        {
            SpriteRenderer sprite = Boss.GetComponentInChildren<SpriteRenderer>();

            if (sprite)
            {
                int textureWidth = (int)(maxBossWidth * 1.25f);
                int textureHeight = (int)(maxBossHeight * 1.25f);

                var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);
                texture.wrapMode = TextureWrapMode.Clamp;

                //TextureDraw.Clear(texture);
                
                int shapeSeed = Random.Range(0, shapeMax);
                int symmetricSeed = Random.Range(0, symmetricMax);
                Debug.Log("Shape seed (0, " + shapeMax + "): " + shapeSeed);
                //Debug.Log("Symmetric seed (0, " + symmetricMax + "): " + symmetricSeed);

                if (shapeSeed < shapeMax / 2)
                {
                    int radiusMax = maxBossWidth / 2;
                    int radiusMin = radiusMax / 10;
                    int radiusSeed = Random.Range(radiusMin, radiusMax);
                    Debug.Log("Radius seed (" + radiusMin + ", " + radiusMax + "): " + radiusSeed);

                    int xMax = textureWidth - radiusSeed;
                    int xMin = radiusSeed;
                    int xSeed = Random.Range(xMin, xMax);
                    Debug.Log("X Origin seed (" + xMin + ", " + xMax + "): " + xSeed);

                    int yMax = textureHeight - radiusSeed;
                    int yMin = radiusSeed;
                    int ySeed = Random.Range(yMin, yMax);
                    Debug.Log("Y Origin seed (" + yMin + ", " + yMax + "): " + ySeed);

                    TextureDraw.Circle(texture, xSeed, ySeed, radiusSeed, Color.red);
                }
                else
                {
                    int minWidth = maxBossWidth / 10;
                    int minHeight = maxBossHeight / 10;
                    int widthSeed = Random.Range(minWidth, maxBossWidth);
                    int heightSeed = Random.Range(minHeight, maxBossHeight);
                    Debug.Log("Width seed (" + minWidth + ", " + maxBossWidth + "): " + widthSeed);
                    Debug.Log("Height seed (" + minHeight + ", " + maxBossHeight + "): " + heightSeed);


                    TextureDraw.Rect(texture, textureWidth / 2, textureHeight / 2, widthSeed, heightSeed, Color.red);
                }


                texture.Apply();

                sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                sprite.enabled = true;
            }
        }
    }
}
