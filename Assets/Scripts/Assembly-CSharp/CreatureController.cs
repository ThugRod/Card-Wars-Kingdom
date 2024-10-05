using System.Collections;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    private Vector3 attackStartPosition;
    private bool initialPositionSet = false;
    private bool shouldReturnToInitial = false;

    // Define LandscapeLiftLerpSpeed with a default value
    public float LandscapeLiftLerpSpeed = 5f;

    public void SetInitialPosition(Vector3 position)
    {
        attackStartPosition = position;
        initialPositionSet = true;
    }

    public void TriggerReturnToInitial()
    {
        shouldReturnToInitial = true;
    }

    private IEnumerator ReturnToInitialPosition()
    {
        while (!initialPositionSet || !shouldReturnToInitial)
        {
            yield return null;
        }

        while (Vector3.Distance(transform.position, attackStartPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                attackStartPosition,
                LandscapeLiftLerpSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = attackStartPosition;
        shouldReturnToInitial = false;
    }

    private void Update()
    {
        if (initialPositionSet && shouldReturnToInitial)
        {
            StartCoroutine(ReturnToInitialPosition());
        }
    }
}


