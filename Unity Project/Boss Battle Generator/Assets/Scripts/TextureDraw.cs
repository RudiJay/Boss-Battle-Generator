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

    public static void Rect(Texture2D texture, int x0, int y0, int width, int height, Color col)
    {
        int dy = height / 2;
        int dx = width / 2;

        for (int y = -dy; y <= dy; y++)
        {
            for (int x = -dx; x <= dx; x++)
            {
                texture.SetPixel(x0 + x, y0 + y, col);
            }
        }
    }
}
