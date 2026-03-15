using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ClockGlowController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] Color enabledDefaultGlow = Color.yellow;
    [SerializeField] Color enabledActiveGlow = Color.green;
    [SerializeField] Color disabledGlow = Color.red;
    
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private RatSpeedUpTrigger clock;    

    void Update()
    {
        float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        alpha *= maxAlpha;

        Color c;
        if (clock.IsActive())
        {            
            if (clock.IsTimeScaledUp())
            {
                c = enabledActiveGlow;
            }
            else
            {
                c= enabledDefaultGlow;
            }
        }
        else
        {
            c = disabledGlow;
        }
        c.a = alpha;
        sr.color = c;
    }
}
