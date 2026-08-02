using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatProgressionUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Slider expSlider;


    private CatProgressionState progressionState;


    public void Bind(CatProgressionState state)
    {
        Unbind();

        progressionState = state;

        if (progressionState == null)
            return;


        progressionState.OnLevelChanged += HandleLevelChanged;
        progressionState.OnExpChanged += HandleExpChanged;

        Refresh();
    }


    public void Unbind()
    {
        if (progressionState == null)
            return;


        progressionState.OnLevelChanged -= HandleLevelChanged;
        progressionState.OnExpChanged -= HandleExpChanged;

        progressionState = null;
    }


    private void HandleLevelChanged(
        int previous,
        int current)
    {
        Refresh();
    }


    private void HandleExpChanged(
        int previous,
        int current)
    {
        Refresh();
    }


    private void Refresh()
    {
        if (progressionState == null)
            return;


        levelText.text = progressionState.CurrentLevel.ToString();


        if (progressionState.ExpToNextLevel <= 0)
        {
            expSlider.value = 1f;
            return;
        }


        expSlider.value =
            (float)progressionState.CurrentExp /
            progressionState.ExpToNextLevel;
    }
}