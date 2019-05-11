using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject bodyObject;

    private bool logicActive = false;

    private IEnumerator bossFightLogicSequence;

    [SerializeField]
    private MovementPatternType[] movementPatternSequence;
    private MovementPatternType currentMovementPattern;

    [SerializeField]
    private float movementSpeed = 1.0f;

    private int maxHealth = 100;
    private int currentHealth;

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
    }

    private void Update()
    {
        //bodyObject.transform.rotation = Quaternion.identity;
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

        bool atDestination = false;

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
            float targetAngle = Vector3.SignedAngle(Vector3.down, targetDirection, Vector3.forward);
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 0.5f);

            float targetDelta = Vector3.Angle(-transform.up, targetDirection);
            if (targetDelta <= 5)
            {
                movementsCompleted++;
                nextDestination = currentMovementPattern.GetNextDestinationPoint(movementsCompleted);
            }

            yield return null;
        }

        yield return null;
    }
}
