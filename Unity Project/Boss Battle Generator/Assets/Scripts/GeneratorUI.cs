using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour
{
    public static GeneratorUI Instance;

    [SerializeField]
    private Text generatingInProgress;
    [SerializeField]
    private InputField seedInputField;

    public void Awake()
    {
        Instance = this;
    }

    public void ToggleGeneratingInProgressLabel(bool value)
    {
        generatingInProgress.enabled = value;
    }

    public void UpdateSeedUI(string inSeed)
    {
        seedInputField.text = inSeed;
    }

    public void SetSeed(string inString)
    {
        if (inString.Length > 0 && inString.Length <= 9)
        {
            GeneratorScript.Instance.SetSeed(int.Parse(inString));
        }
    }

    public void GenerateBossFight()
    {
        GeneratorScript.Instance.GenerateBossFight(true);
    }
}
