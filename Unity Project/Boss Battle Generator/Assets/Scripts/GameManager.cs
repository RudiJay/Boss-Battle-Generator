using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool generatorActive;

    private bool modeTransitionInProgress = false;
    
    [SerializeField]
    private Transform bossSpawn;

    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Transform playerSpawn;

    [SerializeField]
    private Transform battleCamLocation;

    private Transform generatorCamLocation;
    [SerializeField]
    private float generatorCamSize, battleCamSize;
    private bool movingCamera = false;

    [SerializeField]
    private float camMoveTime = 1.0f;

    private PlayerController player;
    private GameObject boss;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        boss = GameObject.FindWithTag("Boss");
        boss.transform.position = bossSpawn.position;

        generatorActive = true;

        generatorCamLocation = GameObject.FindWithTag("Generator").transform;

        if (generatorCamLocation == null)
        {
            Debug.Log("ERROR: Generator Location not found");
        }
    }

    private void Update()
    {
        if (!generatorActive)
        {
            if (Input.GetButton("ExitPlayMode"))
            {
                ExitPlayMode();
            }
        }
    }

    public void SetGeneratorActive(bool value)
    {
        generatorActive = value;
        GeneratorScript.Instance.GeneratorActive = value;
    }

    private IEnumerator CameraTransition(Transform targetLocation, float targetSize)
    {
        movingCamera = true;

        Transform camLocation = Camera.main.transform;
        Vector3 startPos = camLocation.position;

        float startSize = Camera.main.orthographicSize;

        float progress = 0.0f;
        float elapsedTime = 0.0f;

        while (movingCamera)
        {
            elapsedTime += Time.deltaTime;
            progress = elapsedTime / camMoveTime;

            camLocation.position = Vector3.Lerp(startPos, targetLocation.position, progress);

            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, progress);

            if (progress >= 1.0f)
            {
                movingCamera = false;
            }

            yield return null;
        }
    }

    private void CreatePlayer()
    {
        GameObject playerRef = Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation);
        player = playerRef.GetComponent<PlayerController>();
    }

    private void SetPlayerInputEnabled(bool value)
    {
        if (player != null)
        {
            player.InputEnabled = value;
        }
    }

    private void DestroyPlayer()
    {
        Destroy(player.gameObject);
    }

    public void StartBossFight()
    {
        if (!modeTransitionInProgress && generatorActive)
        {
            StartCoroutine(BossFightStartSequence());
        }
    }

    private IEnumerator BossFightStartSequence()
    {
        modeTransitionInProgress = true;

        SetGeneratorActive(false);

        GeneratorUI.Instance.HideUI();

        StartCoroutine(CameraTransition(battleCamLocation, battleCamSize));

        while (movingCamera)
        {
            yield return null;
        }

        CreatePlayer();

        SetPlayerInputEnabled(true);

        GeneratorUI.Instance.SetExitPlayModePromptEnabled(true);

        modeTransitionInProgress = false;
    }

    public void ExitPlayMode()
    {
        if (!modeTransitionInProgress && !generatorActive)
        {
            StartCoroutine(EnterGeneratorSequence());
        }
    }

    private IEnumerator EnterGeneratorSequence()
    {
        modeTransitionInProgress = true;

        GeneratorUI.Instance.SetExitPlayModePromptEnabled(false);

        if (player != null)
        {
            DestroyPlayer();
        }

        StartCoroutine(CameraTransition(generatorCamLocation, generatorCamSize));

        while (movingCamera)
        {
            yield return null;
        }

        boss.transform.position = bossSpawn.position;

        GeneratorUI.Instance.ShowUI();

        SetGeneratorActive(true);

        modeTransitionInProgress = false;
    }
}
