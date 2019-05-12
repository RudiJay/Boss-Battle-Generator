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
    private float movementSpeedMultiplier = 5.0f;
    [SerializeField]
    private float accelerationMultiplier = 1.0f;
    [SerializeField]
    private float minSpeedBuffer = 0.5f;

    private int maxHealth = 100;
    private int currentHealth;

    private int numberOfMovements = 0;
    private Vector3 nextDestination = Vector3.zero;
    private float distanceToNextDestination = 0.0f;
    private WaitForSeconds destinationWaitTime;

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

        destinationWaitTime = new WaitForSeconds(currentMovementPattern.GetWaitTimeAtDestination());
    }

    private IEnumerator BossFightLogicSequence()
    {
        int patternIndex = 0;
        
        int movementsCompleted = 0;
        float currentVelocity = 0.0f;

        SetupNextPattern(patternIndex);

        float timeOnMovement = 0.0f;

        while (logicActive)
        {
            Vector3 targetDirection = nextDestination - transform.position;
            float currentDistance = targetDirection.magnitude;

            timeOnMovement += Time.deltaTime;
            VelocityCurveType accelerationType = currentMovementPattern.GetAccelerationType();
            if (accelerationType.GetAccelerationProportionalToDistanceTravelled())
            {
                if (distanceToNextDestination > 0)
                {
                    currentVelocity = movementSpeedMultiplier * 0.01f * (minSpeedBuffer + accelerationType.GetCurve().Evaluate((distanceToNextDestination - currentDistance) / distanceToNextDestination));
                }
            }
            else
            {
                currentVelocity = movementSpeedMultiplier * 0.01f * accelerationType.GetCurve().Evaluate(timeOnMovement * accelerationMultiplier);
            }

            if (currentDistance <= destinationReachedDistanceThreshold)
            {
                movementsCompleted++;
                timeOnMovement = 0;

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

                yield return destinationWaitTime;
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
