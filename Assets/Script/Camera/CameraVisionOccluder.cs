using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CameraVisionOccluder : MonoBehaviour
{
    // set th
    [SerializeField] SpriteRenderer spriteRenderer;
    [Header("Fade Settings")]
    [SerializeField] float fadeAlpha = 0.3f;
    [SerializeField] float normalAlpha = 1.0f;

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void SetTransparent()
    {
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, fadeAlpha);
    }

    public void Restore()
    {
        Color c = spriteRenderer.color;
        spriteRenderer.color = new Color(c.r, c.g, c.b, normalAlpha);
    }
    
}
