using UnityEngine;
using UnityEngine.UI;

public class TimeRingProgressUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkTimeManager timeManager;
    [SerializeField] private Image ringProgressImage;

    [Header("Color Settings")]
    [SerializeField] private Color ringStartColor = Color.green;
    [SerializeField] private Color ringEndColor = Color.red;
    [SerializeField] private float colorSmoothSpeed = 5f;

    private Color currentRingColor;

    private void Awake()
    {
        currentRingColor = ringStartColor;

        if (timeManager == null)
        {
            timeManager = GetComponent<NetworkTimeManager>();
        }

        if (ringProgressImage != null)
        {
            ringProgressImage.fillAmount = 1f;
            ringProgressImage.color = currentRingColor;
        }
    }

    private void Update()
    {
        if (timeManager == null || ringProgressImage == null) return;

        if (!timeManager.IsCountingDown())
        {
            SetRingVisual(1f);
            return;
        }

        float cycleDuration = timeManager.GetRingTotalCycleTime();
        if (cycleDuration <= 0.001f)
        {
            SetRingVisual(1f);
            return;
        }

        float elapsedInCycle = timeManager.GetElapsedTimeInCycle();
        float progress = 1f - Mathf.Clamp01(elapsedInCycle / cycleDuration);

        SetRingVisual(progress);
    }

    private void SetRingVisual(float progress)
    {
        ringProgressImage.fillAmount = progress;

        Color targetColor = Color.Lerp(ringEndColor, ringStartColor, progress);
        currentRingColor = Color.Lerp(currentRingColor, targetColor, Time.deltaTime * colorSmoothSpeed);
        ringProgressImage.color = currentRingColor;
    }

    public void ResetRing()
    {
        currentRingColor = ringStartColor;

        if (ringProgressImage == null) return;

        ringProgressImage.fillAmount = 1f;
        ringProgressImage.color = currentRingColor;
    }

}