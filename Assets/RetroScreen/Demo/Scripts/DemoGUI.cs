using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NorthLab.Effects;

/// <summary>
/// Allows to change the <see cref="RetroScreen"/> parameters trough GUI.
/// </summary>
public class DemoGUI : MonoBehaviour
{

    private int targetHeight;
    private int fps;

    private void Start()
    {
        targetHeight = RetroScreen.SceneInstance.TargetHeight;
        fps = RetroScreen.SceneInstance.FPS;
    }

    private void OnGUI()
    {
        Rect windowRect = new Rect(0, 0, 250, 120);
        GUI.Window(0, windowRect, Window, "Demo GUI");
    }

    private void Window(int id)
    {
        Rect rect = new Rect(15, 25, 220, 28);
        GUI.Label(rect, "Target Height: " + targetHeight);
        rect.y += 24;
        targetHeight = (int)GUI.HorizontalSlider(rect, targetHeight, 2, 480);
        if (targetHeight != RetroScreen.SceneInstance.TargetHeight)
        {
            RetroScreen.SceneInstance.TargetHeight = targetHeight;
        }

        rect.y += 24;
        GUI.Label(rect, "FPS: " + fps);
        rect.y += 24;
        fps = (int)GUI.HorizontalSlider(rect, fps, 1, 60);
        if (fps != RetroScreen.SceneInstance.FPS)
        {
            RetroScreen.SceneInstance.FPS = fps;
        }
    }

}