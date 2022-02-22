using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorthLab.Effects
{

    /// <summary>
    /// Sends OnPreRender messages to the <see cref="RetroScreen"/> component. Should be attached to the enabled camera to work.
    /// </summary>
    public class RetroScreenCameraAnchor : MonoBehaviour
    {
        
        public RetroScreen Main { get; set; }

        private void OnPreRender()
        {
            Main.ScheduleRender();
        }

    }
}