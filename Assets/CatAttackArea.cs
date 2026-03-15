using System.Collections;
using UnityEngine;

public class CatAttackArea : MonoBehaviour
{
    [SerializeField] private CatPlayerMovement cat;
    [SerializeField] private ThumbSpinner thumbSpinnerPrefab;
    [SerializeField] private Vector3 popupOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float lockDuration = 5.0f;
    [SerializeField] private BuffManager buffManager;
    [SerializeField] private TopDownPlayerMovement ratPlayer;
    private bool ifRatCaught = false;

    private Transform chosenTarget;
    private bool resolved;

    private void OnEnable()
    {
        resolved = false;
        chosenTarget = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (resolved) return;

        // Rat has priority
        if (other.CompareTag("Rat"))
        {
            Debug.Log("Caught the rat");
            Resolve(other.transform, true);
        }
        // NPC only if we haven't hit anything yet
        else if (other.CompareTag("NPC") && chosenTarget == null)
        {
            Resolve(other.transform, false);
        }
    }

    private void Resolve(Transform target, bool thumbUp)
    {
        resolved = true;
        chosenTarget = target;

        // Lock both
        cat.LockMovement(true);


        // Spawn spinner
        if (thumbSpinnerPrefab != null)
        {
            var popup = Instantiate(thumbSpinnerPrefab, target);
            popup.transform.localPosition = popupOffset;
            popup.transform.localRotation = Quaternion.identity;
            popup.Init(thumbUp);
        }
        StartCoroutine(UnlockAfterResolve());
        if (thumbUp)
        {
            ifRatCaught = true;
        }

    }
    private IEnumerator UnlockAfterResolve()
    {
        yield return new WaitForSeconds(lockDuration);
        Unlock();
        resolved = false;
        chosenTarget = null;
        if (ifRatCaught)
        {
            ratPlayer.SetDead(true);
        }
        else {
            buffManager.NotifyFailedToCatchRat();
        } 
    }

    private void Unlock()
    {
        cat.LockMovement(false);//
    }
}