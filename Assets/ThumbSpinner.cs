using UnityEngine;

public class ThumbSpinner : MonoBehaviour
{
    [Header("Sprite")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite thumbUpSprite;

    [Header("Spin Settings")]
    [Tooltip("Total spin duration in seconds")]
    [SerializeField] private float duration = 1.2f;

    [Tooltip("Controls speed over time (fast at start, slow at end)")]
    [SerializeField]
    private AnimationCurve speedOverTime =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Tooltip("Maximum rotation speed in degrees per second")]
    [SerializeField] private float maxDegreesPerSecond = 1400f;

    private bool finalThumbUp;
    private float elapsed;
    private float currentAngle;
    private bool running;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null && thumbUpSprite != null)
            sr.sprite = thumbUpSprite;
        Init(true);
    }


    public void Init(bool endOnThumbUp)
    {
        finalThumbUp = endOnThumbUp;
        elapsed = 0f;
        currentAngle = 0f;
        running = true;

        transform.localEulerAngles = Vector3.zero;
    }

    private void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        float speed01 = speedOverTime.Evaluate(t);
        float degreesPerSecond = maxDegreesPerSecond * speed01;

        currentAngle += degreesPerSecond * Time.deltaTime;

        transform.localEulerAngles = new Vector3(currentAngle, 0f, 0f);

        if (t >= 1f)
        {
            running = false;
            transform.localEulerAngles = finalThumbUp
                ? Vector3.zero
                : new Vector3(180f, 0f, 0f);

            Destroy(gameObject, 0.4f);
        }
    }
}
