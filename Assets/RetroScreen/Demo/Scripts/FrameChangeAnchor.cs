using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NorthLab.Effects;

/// <summary>
/// Increments number every rendered frame.
/// </summary>
public class FrameChangeAnchor : MonoBehaviour
{

    [SerializeField]
    private TextMesh text = null;

    private int number = 0;

    private void OnEnable()
    {
        RetroScreen.onFrameRendered += OnFrameRendered;
    }

    private void OnDisable()
    {
        RetroScreen.onFrameRendered -= OnFrameRendered;
    }

    private void OnFrameRendered(RetroScreen sender)
    {
        if (++number > 9)
            number = 0;
        text.text = number.ToString();
    }

}
