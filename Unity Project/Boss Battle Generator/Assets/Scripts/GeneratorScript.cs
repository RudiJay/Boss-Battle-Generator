using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    public static GeneratorScript Instance;

    private GameObject Boss;

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
                var texture = new Texture2D(8, 10, TextureFormat.ARGB32, false);

                for (int y = 0; y < texture.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        if (x % 2 == 0)
                        {
                            texture.SetPixel(x, y, Color.red);
                        }
                        else
                        {
                            texture.SetPixel(x, y, Color.black);
                        }
                    }
                }

                texture.Apply();

                sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                sprite.enabled = true;

                //sprite.color = Random.ColorHSV();

                //if (Time.frameCount % 5 == 0)
                //{
                //    sprite.color = Color.red;
                //}
            }
        }
    }
}
