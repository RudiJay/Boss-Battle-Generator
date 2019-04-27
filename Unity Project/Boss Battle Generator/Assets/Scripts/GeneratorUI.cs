/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour
{
    public static GeneratorUI Instance;

    private bool uiVisible = true;

    [SerializeField]
    private Color textColor, highlightColor;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private Text generatingInProgress;
    [SerializeField]
    private InputField seedInputField;
    [SerializeField]
    private Dropdown bossTypeDropdown;
    [SerializeField]
    private Text currentAttackLabel, attackPatternSizeLabel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PopulateBossTypeDropdown();
    }

    public void ToggleUI()
    {
        uiVisible = !uiVisible;

        if (!uiVisible)
        {
            HideUI();
        }
        else
        {
            ShowUI();
        }
    }

    public void HideUI()
    {
        canvasGroup.alpha = 0.0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void ShowUI()
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
    }

    public void PopulateBossTypeDropdown()
    {
        List<string> dropdownOptions = new List<string>();

        for (int i = 0; i < System.Enum.GetNames(typeof(BossTypeName)).Length; i++)
        {
            dropdownOptions.Add(((BossTypeName)i).ToString());
        }

        bossTypeDropdown.AddOptions(dropdownOptions);
    }

    public void ShowRandomBossType(string bossType)
    {
        bossTypeDropdown.captionText.text = "Random: " + bossType;
    }

    public BossTypeName GetBossTypeName()
    {
        return (BossTypeName)bossTypeDropdown.value;
    }

    public void SetGeneratingInProgressLabel(bool value)
    {
        generatingInProgress.enabled = value;
    }

    public void SetCurrentAttack(int value)
    {
        currentAttackLabel.text = value.ToString();
    }

    public void SetAttackPatternSize(int value)
    {
        attackPatternSizeLabel.text = value.ToString();
    }

    public void SetCurrentlyDemonstratingAttacks(bool value)
    {
        if (value)
        {
            currentAttackLabel.color = highlightColor;
        }
        else
        {
            currentAttackLabel.color = textColor;
        }

    }

    public void UpdateSeedUI(string seedString)
    {
        seedInputField.text = seedString;
    }

    public void SetSeed(string inString)
    {
        if (inString.Length > 0 && inString.Length <= 9)
        {
            GeneratorScript.Instance.SetSeed(int.Parse(inString));
        }
    }

    public void GenerateBossFight(bool useNewSeed)
    {
        if (useNewSeed)
        {
            GeneratorScript.Instance.GenerateBossFight(true);
        }
        else
        {
            SetSeed(seedInputField.text);
        }
    }
}
