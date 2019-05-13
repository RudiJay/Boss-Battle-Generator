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
    private bool performingAttacks = false;

    private IEnumerator bossAttackSequence, bossMovementSequence;

    private List<IAttackType> attackSequence;

    private List<MovementPatternType> movementPatternSequence;
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

    [SerializeField]
    private float delayBetweenAttackSequenceLoop;
    private WaitForSeconds delayBetweenAttackSequenceLoopTime;
    private WaitForSeconds delayBetweenAttackTime;

    public bool GetCurrentlyPerformingAttackSequence()
    {
        return performingAttacks;
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
    }

    private void Start()
    {
        bossAttackSequence = BossAttackSequenceLogic();
        bossMovementSequence = BossMovementSequenceLogic();

        attackSequence = new List<IAttackType>();
        movementPatternSequence = new List<MovementPatternType>();
    }

    public void StartAttackSequence()
    {
        performingAttacks = true;

        UIManager.Instance.SetCurrentlyPerformingAttacks(true);

        bossAttackSequence = BossAttackSequenceLogic();
        StartCoroutine(bossAttackSequence);
    }

    public void StopAttackSequence()
    {
        performingAttacks = false;

        UIManager.Instance.SetCurrentlyPerformingAttacks(false);

        StopCoroutine(bossAttackSequence);
    }

    public void StartBossFight()
    {
        currentHealth = maxHealth;

        logicActive = true;

        UIManager.Instance.SetCurrentlyPerformingMovement(true);

        bossMovementSequence = BossMovementSequenceLogic();
        StartCoroutine(bossMovementSequence);
    }

    public void StopBossFight()
    {
        logicActive = false;

        UIManager.Instance.SetCurrentlyPerformingMovement(false);

        StopCoroutine(bossMovementSequence);
    }

    public void SetupAttackSequence(List<IAttackType> sequence)
    {
        attackSequence = sequence;
    }

    public void SetupMovementPatternSequence(List<MovementPatternType> sequence)
    {
        movementPatternSequence = sequence;
    }

    private IEnumerator BossAttackSequenceLogic()
    {
        UIManager.Instance.SetAttackSequenceSize(attackSequence.Count);
        delayBetweenAttackSequenceLoopTime = new WaitForSeconds(delayBetweenAttackSequenceLoop);

        while (performingAttacks)
        {
            for (int i = 0; i < attackSequence.Count; i++)
            {
                UIManager.Instance.SetCurrentAttack(i + 1);

                delayBetweenAttackTime = new WaitForSeconds(attackSequence[i].DelayAfterAttack);

                attackSequence[i].PerformAttack();

                yield return delayBetweenAttackTime;
            }

            yield return delayBetweenAttackSequenceLoopTime;
        }
    }

    private IEnumerator BossMovementSequenceLogic()
    {
        UIManager.Instance.SetMovementPatternSequenceSize(movementPatternSequence.Count);

        WaitForSeconds destinationWaitTime;

        Vector3 nextDestination;
        Vector3 targetDirection;

        float distanceToNextDestination = 0.0f;
        float currentDistance = 0.0f;

        float currentVelocity = 0.0f;

        float timeOnMovement = 0.0f;

        while (logicActive)
        {
            for (int i = 0; i < movementPatternSequence.Count; i++)
            {
                UIManager.Instance.SetCurrentMovementPattern(i + 1);

                currentMovementPattern = movementPatternSequence[i];
                if (currentMovementPattern.includeStartPointInDestinations)
                {
                    currentMovementPattern.SetStartPoint(transform.position);
                }
                destinationWaitTime = new WaitForSeconds(currentMovementPattern.waitTimeAtDestination);

                VelocityCurveType accelerationType = currentMovementPattern.accelerationType;
                if (accelerationType == null)
                {
                    Debug.Log("Missing acceleration type");
                    yield break;
                }

                for (int j = 0; j < currentMovementPattern.numberOfMovements; j++)
                {
                    nextDestination = currentMovementPattern.GetNextDestinationPoint(j);
                    targetDirection = nextDestination - transform.position;

                    distanceToNextDestination = targetDirection.magnitude;

                    currentDistance = distanceToNextDestination;

                    while (currentDistance > destinationReachedDistanceThreshold)
                    {
                        targetDirection = nextDestination - transform.position;
                        currentDistance = targetDirection.magnitude;

                        timeOnMovement += Time.deltaTime;

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

                        rb.MovePosition(transform.position + (targetDirection.normalized * currentVelocity));

                        yield return null;
                    }

                    timeOnMovement = 0;

                    yield return destinationWaitTime;
                }
            }
        }

        yield return null;
    }
}
