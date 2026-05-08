using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "LANMenu";
    [SerializeField] private PlayableDirector director;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private bool isSkipping;
    void Update()
    {
        if (director == null) return;

        bool isPlaying = director.state == PlayState.Playing;

        if (!isPlaying) return;
        
        if (Input.GetKeyDown(skipKey))
        {
            OnSkipScenePressed();
        }

    }
    public void OnTimelineTriggerSceneSwitch()
    {
        if (String.IsNullOrEmpty(lobbySceneName))
        {
            return;
        }

        SceneManager.LoadScene(lobbySceneName);
    }

    private void OnSkipScenePressed()
    {
        if (isSkipping) return;
        isSkipping = true;

        director.time = director.duration;
        director.Evaluate();
        director.Stop();

    }
}
