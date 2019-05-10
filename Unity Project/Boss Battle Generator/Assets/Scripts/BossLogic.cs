using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLogic : MonoBehaviour
{
    private int maxHealth = 100;
    private int currentHealth;

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
    }

    public void StartBossFight()
    {
        currentHealth = maxHealth;
    }
}
