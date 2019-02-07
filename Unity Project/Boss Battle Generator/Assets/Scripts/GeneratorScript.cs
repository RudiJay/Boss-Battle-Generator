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
                sprite.enabled = true;

                sprite.color = Random.ColorHSV();

                if (Time.frameCount % 5 == 0)
                {
                    sprite.color = Color.red;
                }
            }
        }
    }
}
