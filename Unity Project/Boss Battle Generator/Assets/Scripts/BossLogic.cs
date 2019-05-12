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
    private float movementSpeed = 5.0f;
    [SerializeField]
    private float accelerationFactor = 1.0f;

    private int maxHealth = 100;
    private int currentHealth;

    private int numberOfMovements = 0;
    private Vector3 nextDestination = Vector3.zero;
    private float distanceToNextDestination = 0.0f;

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

    private void SetupNextPattern(int patternIndex)
    {
        currentMovementPattern = movementPatternSequence[patternIndex];

        numberOfMovements = currentMovementPattern.GetNumberOfMovements();
        nextDestination = currentMovementPattern.GetNextDestinationPoint(0);
        distanceToNextDestination = (nextDestination - transform.position).magnitude;

        if (currentMovementPattern.GetIncludeStartPoint())
        {
            currentMovementPattern.SetStartPoint(transform.position);
        }
    }

    private IEnumerator BossFightLogicSequence()
    {
        int patternIndex = 0;
        
        int movementsCompleted = 0;
        float currentVelocity = 0.0f;

        SetupNextPattern(patternIndex);
        bool changed = true;

        float timeOnMovement = 0.0f;

        while (logicActive)
        {
            if (changed)
            {
                Debug.Log(movementsCompleted+1 + "/" + numberOfMovements + " " + nextDestination);
                changed = false;
            }

            Vector3 targetDirection = nextDestination - transform.position;
            float currentDistance = targetDirection.magnitude;

            timeOnMovement += Time.deltaTime;
            currentVelocity = movementSpeed * Time.deltaTime * currentMovementPattern.GetAccelerationType().GetCurve().Evaluate(timeOnMovement * accelerationFactor);

            if (currentDistance <= destinationReachedDistanceThreshold)
            {
                movementsCompleted++;

                if (movementsCompleted >= numberOfMovements)
                {
                    patternIndex++;
                    if (patternIndex >= movementPatternSequence.Length)
                    {
                        patternIndex = 0;
                    }

                    movementsCompleted = 0;

                    SetupNextPattern(patternIndex);
                }
                else
                {
                    nextDestination = currentMovementPattern.GetNextDestinationPoint(movementsCompleted);
                    distanceToNextDestination = (nextDestination - transform.position).magnitude;
                }
                changed = true;
            }
            else
            {
                rb.MovePosition(transform.position + (targetDirection.normalized * currentVelocity));
            }

            yield return null;
        }

        yield return null;
    }
}
