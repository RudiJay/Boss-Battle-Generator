﻿/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */
 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private bool generatorUIVisible = true;
    private bool indicatorsVisible = true;

    [SerializeField]
    private Color textColor, highlightColor;

    [SerializeField]
    private CanvasGroup generationModeCanvasGroup, playModeCanvasGroup, indicatorCanvasGroup;

    [SerializeField]
    private Button playModeButton;
    [SerializeField]
    private Text generatingInProgress;
    [SerializeField]
    private InputField seedInputField;
    [SerializeField]
    private Dropdown bossTypeDropdown;
    [SerializeField]
    private Text attackQuantityLabel, currentAttackLabel, attackSequenceLengthLabel;
    [SerializeField]
    private Text currentMovementPatternLabel, movementPatternSequenceLengthLabel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PopulateBossTypeDropdown();

        playModeButton.interactable = false;
    }

    public void ToggleGeneratorUI()
    {
        generatorUIVisible = !generatorUIVisible;

        ShowGeneratorUI(generatorUIVisible);
    }

    public void ToggleIndicators()
    {
        indicatorsVisible = !indicatorsVisible;

        ShowIndicators(indicatorsVisible);
    }

    public void ShowGeneratorUI(bool value)
    {
        generatorUIVisible = value;
        if (value)
        {
            generationModeCanvasGroup.alpha = 1.0f;
            generationModeCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            generationModeCanvasGroup.alpha = 0.0f;
            generationModeCanvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowPlayModeUI(bool value)
    {
        if (value)
        {
            playModeCanvasGroup.alpha = 1.0f;
        }
        else
        {
            playModeCanvasGroup.alpha = 0.0f;
        }
    }

    public void ShowIndicators(bool value)
    {
        indicatorsVisible = value;
        if (value)
        {
            indicatorCanvasGroup.alpha = 1.0f;
        }
        else
        {
            indicatorCanvasGroup.alpha = 0.0f;
        }
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

    public void SetGeneratingInProgressLabelEnabled(bool value)
    {
        generatingInProgress.enabled = value;

        playModeButton.interactable = !value;
    }

    public void ResetAttackUI()
    {
        attackQuantityLabel.text = "?";

        currentAttackLabel.text = "0";
        attackSequenceLengthLabel.text = "?";
    }

    public void ResetMovementPatternUI()
    {
        currentMovementPatternLabel.text = "0";
        movementPatternSequenceLengthLabel.text = "?";
    }

    public void SetAttackQuantity(int value)
    {
        attackQuantityLabel.text = value.ToString();
    }

    public void SetCurrentAttack(int value)
    {
        currentAttackLabel.text = value.ToString();
    }

    public void SetAttackSequenceSize(int value)
    {
        attackSequenceLengthLabel.text = value.ToString();
    }

    public void SetCurrentlyPerformingAttacks(bool value)
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

    public void SetCurrentMovementPattern(int value)
    {
        currentMovementPatternLabel.text = value.ToString();
    }

    public void SetMovementPatternSequenceSize(int value)
    {
        movementPatternSequenceLengthLabel.text = value.ToString();
    }

    public void SetCurrentlyPerformingMovement(bool value)
    {
        if (value)
        {
            currentMovementPatternLabel.color = highlightColor;
        }
        else
        {
            currentMovementPatternLabel.color = textColor;
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
        else if (seedInputField.text.Length > 0)
        {
            SetSeed(seedInputField.text);
            GeneratorScript.Instance.GenerateBossFight(false);            
        }
    }

    public void PlayBossFight()
    {
        GameManager.Instance.StartBossFight();
    }
}