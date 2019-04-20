/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureDraw
{
    /// <summary>
    /// Gets result of a multiply blend of the colors of two pixels
    /// </summary>
    /// <param name="shapeCol">First pixel to blend</param>
    /// <param name="overlayCol">Second pixel blend onto shape</param>
    /// <returns>The blended result color</returns>
    public static Color MultiplyBlendPixel(Color shapeCol, Color overlayCol)
    {
        Color finalCol;

        finalCol.r = (shapeCol.r * overlayCol.r) / 2.0f;
        finalCol.g = (shapeCol.g * overlayCol.g) / 2.0f;
        finalCol.b = (shapeCol.b * overlayCol.b) / 2.0f;
        finalCol.a = shapeCol.a;

        return finalCol;
    }

    /// <summary>
    /// Makes every pixel in the texture transparent
    /// </summary>
    /// <param name="ioTexture">The texture to clear</param>
    public static void Clear(Texture2D ioTexture)
    {
        for (int y = 0; y < ioTexture.height; y++)
        {
            for (int x = 0; x < ioTexture.width; x++)
            {
                ioTexture.SetPixel(x, y, Color.clear);
            }
        }
    }

    /// <summary>
    /// Adds a shadow outline to the non-transparent parts of a texture
    /// </summary>
    /// <param name="inTexture">the texture read into the outlined output</param>
    /// <param name="outlineColor">the color of the outline</param>
    /// <param name="outlineWidth">the width in pixels of the outline</param>
    /// <returns>the outlined texture</returns>
    public static Texture2D OutlineTexture(Texture2D inTexture, Color outlineColor, int outlineWidth)
    {
        Texture2D outTexture = new Texture2D(inTexture.width, inTexture.height);

        Clear(outTexture);

        for (int y = outlineWidth; y < inTexture.height - outlineWidth; y++)
        {
            for (int x = outlineWidth; x < inTexture.width - outlineWidth; x++)
            {
                if (inTexture.GetPixel(x, y).a != 0.0f)
                {
                    for (int y2 = -outlineWidth; y2 <= outlineWidth; y2++)
                    {
                        for (int x2 = -outlineWidth; x2 <= outlineWidth; x2++)
                        {
                            outTexture.SetPixel(x + x2, y + y2, outlineColor);
                        }
                    }
                }
            }
        }

        for (int y = 0; y < inTexture.height; y++)
        {
            for (int x = 0; x < inTexture.width; x++)
            {
                Color pixelCol = inTexture.GetPixel(x, y);
                if (pixelCol.a != 0.0f)
                {
                    outTexture.SetPixel(x, y, pixelCol);
                }
            }
        }

        outTexture.Apply();

        return outTexture;
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
                    textureIO.SetPixel(x0 + x, y0 + y, Color.white);
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
