using System.Collections;
using TMPro;
using UnityEngine;

public class CountDownUI : MonoBehaviour
{
    [SerializeField] private TextMeshPro tmp;
    [SerializeField] private float stepSeconds = 1f;
    [SerializeField] private int countDownDuration = 5;
    [SerializeField] private GameObject countDownObject;
    private Coroutine routine;

    void Update()
    {
        Vector3 baseScale = transform.localScale;
        var s = transform.lossyScale;
        float signX = Mathf.Sign(s.x);
        transform.localScale = new Vector3(baseScale.x * signX, baseScale.y, baseScale.z);
    }
    [ContextMenu("TEST: Start Countdown")]
    public void StartCountdown()
    {
        if (routine != null) StopCoroutine(routine);

        routine = StartCoroutine(CountdownRoutine(countDownDuration));
    }

    public void StopCountdown()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
    }

    private IEnumerator CountdownRoutine(int from)
    {
        countDownObject.SetActive(true);
        for (int i = from; i >= 0; i--)
        {
            if (tmp != null) tmp.text = i.ToString();

            // Wait exactly stepSeconds
            yield return new WaitForSeconds(stepSeconds);
        }
        countDownObject.SetActive(false);
    }
}
