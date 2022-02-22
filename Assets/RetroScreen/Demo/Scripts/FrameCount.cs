using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NorthLab.Effects;

public class FrameCount : MonoBehaviour
{

    private float prevFrameTime;
    private float frameDelay;
    private float FPS => 1f / frameDelay;
    private int targetDiff = 0;
    private float realFPS;

    private static readonly string[] VsyncLevels = new string[]
    {
        "Don't sync", "Every V blank", "Every second V blank"
    };

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
    }

    private void OnEnable()
    {
        RetroScreen.onFrameRendered += OnFrameChanged;
    }

    private void OnDisable()
    {
        RetroScreen.onFrameRendered -= OnFrameChanged;
    }

    private void OnGUI()
    {
        GUI.contentColor = Color.black;
        //Big ass debug message
        GUI.Label(new Rect(5, 0, 300, 300), $"Vsync - {VsyncLevels[QualitySettings.vSyncCount]}\nPress Num+ and Num- to change Vsync\n\nReal FPS - {realFPS.ToString("F0")}\nFrameDelay - {frameDelay.ToString("F3")}\nFPS - {FPS.ToString("F0")}\nTarget FPS diff - {targetDiff}");
    }

    private void Update()
    {
        realFPS = 1f / Time.unscaledDeltaTime;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            ChangeVsyncLevel(1);
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            ChangeVsyncLevel(-1);
        }
    }

    private void ChangeVsyncLevel(int change)
    {
        int newLevel = QualitySettings.vSyncCount + change;
        if (newLevel < 0)
        {
            newLevel = 0;
        }
        else if (newLevel > 2)
        {
            newLevel = 2;
        }

        QualitySettings.vSyncCount = newLevel;
    }

    private void OnFrameChanged(RetroScreen sender)
    {
        frameDelay = sender.FrameRenderTimestamp - prevFrameTime;
        prevFrameTime = sender.FrameRenderTimestamp;
        targetDiff = sender.FPS - (int)FPS;
    }

}
