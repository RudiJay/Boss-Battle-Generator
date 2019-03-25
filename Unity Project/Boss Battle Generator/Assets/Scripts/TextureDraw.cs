/* 
 * Copyright (C) 2018 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureDraw
{
    /// <summary>
    /// Makes every pixel in the texture transparent
    /// </summary>
    /// <param name="texture"></param>
    public static void Clear(Texture2D texture)
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }
    }

    /// <summary>
    /// Copy pixels from one texture onto another at a position
    /// </summary>
    /// <param name="textureIO">texture to copy other texture onto</param>
    /// <param name="textureIn">texture to copy onto other texture</param>
    /// <param name="x0">x position to start copying texture at</param>
    /// <param name="y0">y position to start copying texture at</param>
    public static void CopyFromTexture(Texture2D textureIO, Texture2D textureIn, int x0, int y0)
    {
        for (int y = 0; y < textureIn.height; y++)
        {
            for (int x = 0; x < textureIn.width; x++)
            {
                if (textureIn.GetPixel(x, y).a == 1)
                {
                    textureIO.SetPixel(x0 + x, y0 + y, textureIn.GetPixel(x, y));
                }
            }
        }
    }

    /// <summary>
    /// Draws a circle on the texture
    /// </summary>
    /// <param name="texture">texture to draw circle onto</param>
    /// <param name="x0">x coordinate of midpoint of circle</param>
    /// <param name="y0">y coordinate of midpoint of circle</param>
    /// <param name="radius">radius of circle</param>
    /// <param name="col">color of the circle</param>
    public static void Circle(Texture2D texture, int x0, int y0, int radius, Color col)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x*x + y*y <= radius*radius + radius)
                {
                    texture.SetPixel(x0 + x, y0 + y, col);
                }
            }
        }
    }
    /// <summary>
    /// Draws a rectangle on the texture
    /// </summary>
    /// <param name="texture">texture to draw rect onto</param>
    /// <param name="x0">x coordinate of midpoint of rect</param>
    /// <param name="y0">y coordinate of midpoint of rect</param>
    /// <param name="width">total width of rect</param>
    /// <param name="height">total height of rect</param>
    /// <param name="col">color of rect</param>
    /// <param name="rotation">rotation of rect (known issues)</param>
    public static void Rect(Texture2D texture, int x0, int y0, int width, int height, Color col, int rotation = 0)
    {
        int dy = height / 2;
        int dx = width / 2;

        for (int y = -dy; y <= dy; y++)
        {
            for (int x = -dx; x <= dx; x++)
            {
                int rotX = x0 + x;
                int rotY = y0 + y;

                if (rotation != 0)
                {
                    rotX = (int)((x0 + x) * Mathf.Cos(rotation) - (y0 + y) * Mathf.Sin(rotation));
                    rotY = (int)((x0 + x) * Mathf.Sin(rotation) + (y0 + y) * Mathf.Cos(rotation));
                }

                texture.SetPixel(rotX, rotY, col);
            }
        }
    }
}
