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

        if (Input.GetButtonDown("ToggleUI"))
        {
            if (generatorActive)
            {
                UIManager.Instance.ToggleGeneratorUI();
            }

            UIManager.Instance.ToggleIndicators();
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

    public void SetBossLife(int value)
    {
        bossLogic.SetMaxLife(value);
    }

    public void SetBossSpeed(float value)
    {
        bossLogic.SetMovementSpeedModifier(value);
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
        playerController.SetBossTransform(boss.transform);
        ProjectileManager.Instance.SendPlayerTransformToProjectiles(player.transform);
    }

    private void DisablePlayer()
    {
        player.SetActive(false);
    }

    public void StartAttackSequence()
    {
        if (!bossLogic.GetCurrentlyPerformingAttackSequence())
        {
            bossLogic.StartAttackSequence();
        }
    }

    public void StopAttackSequence()
    {
        bossLogic.StopAttackSequence();
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

        UIManager.Instance.ShowGeneratorUI(false);

        UIManager.Instance.ShowPlayModeUI(true);

        StartCoroutine(CameraTransition(battleCamLocation, battleCamSize));

        EnablePlayer();

        while (movingCamera)
        {
            yield return null;
        }

        playerController.SetUpEdgeBoundaries(battleCamLocation.position);

        SetPlayerInputEnabled(true);

        if (!bossLogic.GetCurrentlyPerformingAttackSequence())
        {
            bossLogic.StartAttackSequence();
        }

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

        UIManager.Instance.ShowPlayModeUI(false);

        bossLogic.StopAttackSequence();
        bossLogic.StopBossFight();

        StartCoroutine(CameraTransition(generatorCamLocation, generatorCamSize));

        while (movingCamera)
        {
            yield return null;
        }

        DisablePlayer();

        ProjectileManager.Instance.SendPlayerTransformToProjectiles(null);
        ProjectileManager.Instance.DisableAllProjectiles();

        boss.transform.position = bossSpawn.position;
        bossLogic.ResetBoss();

        UIManager.Instance.ShowGeneratorUI(true);
        UIManager.Instance.ShowIndicators(true);

        SetGeneratorActive(true);

        bossLogic.StartAttackSequence();

        modeTransitionInProgress = exitingPlayMode = false;
    }
}
