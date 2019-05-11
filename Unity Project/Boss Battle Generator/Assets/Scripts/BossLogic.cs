using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject bodyObject;
    [SerializeField]
    private Rigidbody2D rb;

    private bool logicActive = false;

    private IEnumerator bossFightLogicSequence;

    [SerializeField]
    private MovementPatternType[] movementPatternSequence;
    private MovementPatternType currentMovementPattern;

    [SerializeField]
    private float destinationReachedDistanceThreshold = 0.5f;

    [SerializeField]
    private float movementSpeed = 1.0f;

    private int maxHealth = 100;
    private int currentHealth;

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
    }

    public void StartBossFight()
    {
        currentHealth = maxHealth;

        logicActive = true;

        bossFightLogicSequence = BossFightLogicSequence();
        StartCoroutine(bossFightLogicSequence);
    }

    public void StopBossFight()
    {
        logicActive = false;
        StopCoroutine(bossFightLogicSequence);
    }

    private IEnumerator BossFightLogicSequence()
    {
        int patternIndex = -1;

        int numberOfMovements = 0;
        int movementsCompleted = 0;
        Vector3 nextDestination = Vector3.zero;

        while (logicActive)
        {
            if (movementsCompleted >= numberOfMovements)
            {
                patternIndex++;
                if (patternIndex >= movementPatternSequence.Length)
                {
                    patternIndex = 0;
                }

                currentMovementPattern = movementPatternSequence[patternIndex];

                numberOfMovements = currentMovementPattern.GetNumberOfMovements();
                movementsCompleted = 0;
                nextDestination = currentMovementPattern.GetNextDestinationPoint(movementsCompleted);

                if (currentMovementPattern.GetIncludeStartPoint())
                {
                    currentMovementPattern.SetStartPoint(transform.position);
                }
            }

            Vector3 targetDirection = nextDestination - transform.position;
            
            if (targetDirection.magnitude <= destinationReachedDistanceThreshold)
            {
                movementsCompleted++;
                nextDestination = currentMovementPattern.GetNextDestinationPoint(movementsCompleted);
            }
            else
            {
                rb.MovePosition(transform.position + (targetDirection.normalized * movementSpeed * Time.deltaTime));
            }

            yield return null;
        }

        yield return null;
    }
}
