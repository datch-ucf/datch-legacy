/*
 * This script is used to increase and decrease the line weight
 * 
 */

using HoloToolkit.Examples.InteractiveElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Examples.GazeRuler;
using UnityEngine.UI;

public class LineWeight : Interactive {


    public static float lineWeight = 0.005f;
    public float minWeight = 0.005f;
    public float maxWeight = 0.01f;

    public float weight = .005f;

    public Toggle objectToDisable;
    // Use this for initialization
    protected override void Start ()
    {
        base.Start();

        
	}

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        //lineWeight = weight;
    }

    public void Increase()
    {
        if(lineWeight >= maxWeight)
        {
            lineWeight = maxWeight;
        }
        else
        {
            lineWeight += .001f;
        }
    }

    public void Decrease()
    {
        if (lineWeight <= minWeight)
        {
            lineWeight = minWeight;
        }
        else
        {
            lineWeight -= .001f;
        }
    }

    public override void OnInputClicked(InputClickedEventData eventData)
    {
        base.OnInputClicked(eventData);
        print("Clicked");
        
        eventData.Use();
        //StartCoroutine(MeasureManager.Instance.WaitToSetDrawing(true, this.gameObject));
    }

    public override void OnHold()
    {
        base.OnHold();
    }

    public void SetLineWeight(float weight)
    {
        lineWeight = weight;
        print(lineWeight);
        MeasureManager.Instance.SetDrawing(true, null);
    }
}

