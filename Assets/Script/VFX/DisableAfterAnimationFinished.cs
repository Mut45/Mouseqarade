using UnityEngine;

public class DisableAfterAnimationFinished : MonoBehaviour
{
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
