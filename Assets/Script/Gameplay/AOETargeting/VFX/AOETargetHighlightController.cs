using UnityEngine;
public class AOETargetHighlightController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.yellow;
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    public void SetAoeHighlighted(bool highlighted)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = highlighted ? highlightedColor : normalColor;
    }

}