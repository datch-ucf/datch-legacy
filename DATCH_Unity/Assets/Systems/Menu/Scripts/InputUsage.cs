/*
 * I will use this for hololens input is not being used in a script but needs it to function
 * so I don't have to rewrite it everytime in individual scripts
 */

using HoloToolkit.Examples.GazeRuler;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputUsage : MonoBehaviour, IInputClickHandler
{   
    public void OnInputClicked(InputClickedEventData eventData)
    {
        MeasureManager.Instance.SetDrawing(false);
        eventData.Use();// Mark the event as used, so it doesn't fall through to other handlers.
        MeasureManager.Instance.SetDrawing(true, null);
    }
}
