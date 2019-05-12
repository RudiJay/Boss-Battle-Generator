/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */
 
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
    private CanvasGroup generationModeCanvasGroup, playModeCanvasGroup;

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
        uiVisible = !uiVisible;

        ShowGeneratorUI(uiVisible);
    }

    public void ShowGeneratorUI(bool value)
    {
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
            playModeCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            playModeCanvasGroup.alpha = 0.0f;
            playModeCanvasGroup.blocksRaycasts = false;
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
