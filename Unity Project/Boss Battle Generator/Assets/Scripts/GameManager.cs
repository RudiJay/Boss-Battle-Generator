/* 
 * Copyright (C) 2019 Rudi Jay Prentice - All right reserved
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool generatorActive = true;
    private static List<Action> generatorListeners;

    private bool modeTransitionInProgress = false;
    private bool exitingPlayMode = false;

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

    private GameObject boss;
    private BossLogic bossLogic;

    private GameObject player;
    private PlayerController playerController;
    

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        boss = GameObject.FindWithTag("Boss");
        boss.transform.position = bossSpawn.position;
        bossLogic = boss.GetComponent<BossLogic>();

        generatorActive = true;

        generatorCamLocation = GameObject.FindWithTag("Generator").transform;

        if (generatorCamLocation == null)
        {
            Debug.Log("ERROR: Generator Location not found");
        }

        player = Instantiate(playerPrefab);
        playerController = player.GetComponent<PlayerController>();
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

    public void RegisterAsGeneratorListener(Action actionToRegister)
    {
        if (generatorListeners == null)
        {
            generatorListeners = new List<Action>();
        }

        generatorListeners.Add(actionToRegister);
    }

    public void DeregisterAsGeneratorListener(Action actionToDeregister)
    {
        if (generatorListeners.Contains(actionToDeregister))
        {
            generatorListeners.Remove(actionToDeregister);
        }
    }

    private void UpdateGeneratorListeners()
    {
        if (generatorListeners != null)
        {
            foreach (Action listener in generatorListeners)
            {
                listener();
            }
        }
    }

    public void SetGeneratorActive(bool value)
    {
        generatorActive = value;
        UpdateGeneratorListeners();
    }

    public bool GetGeneratorActive()
    {
        return generatorActive;
    }

    public Transform GetPlayerTransform()
    {
        if (player != null)
        {
            return player.transform;
        }

        return null;
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

    private void SetPlayerInputEnabled(bool value)
    {
        if (playerController != null)
        {
            playerController.InputEnabled = value;
        }
    }

    private void EnablePlayer()
    {
        player.transform.SetPositionAndRotation(playerSpawn.position, playerSpawn.rotation);
        player.SetActive(true);
    }

    private void DisablePlayer()
    {
        player.SetActive(false);
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

        GeneratorUI.Instance.ShowGeneratorUI(false);

        GeneratorUI.Instance.ShowPlayModeUI(true);

        StartCoroutine(CameraTransition(battleCamLocation, battleCamSize));

        EnablePlayer();

        while (movingCamera)
        {
            yield return null;
        }

        playerController.SetUpEdgeBoundaries(battleCamLocation.position);

        SetPlayerInputEnabled(true);

        bossLogic.StartBossFight();

        modeTransitionInProgress = false;
    }

    public void ExitPlayMode()
    {
        if (!generatorActive && !exitingPlayMode)
        {
            StopAllCoroutines();
            StartCoroutine(EnterGeneratorSequence());
        }
    }

    private IEnumerator EnterGeneratorSequence()
    {
        modeTransitionInProgress = exitingPlayMode = true;

        GeneratorUI.Instance.ShowPlayModeUI(false);

        bossLogic.StopBossFight();

        StartCoroutine(CameraTransition(generatorCamLocation, generatorCamSize));

        while (movingCamera)
        {
            yield return null;
        }

        DisablePlayer();

        ProjectileManager.Instance.DisableAllProjectiles();

        boss.transform.position = bossSpawn.position;

        GeneratorUI.Instance.ShowGeneratorUI(true);

        SetGeneratorActive(true);

        modeTransitionInProgress = exitingPlayMode = false;
    }
}
