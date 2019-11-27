using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameRate : MonoBehaviour {
    Text frameRate;
    float deltaTime = 0.0f;
    float refreshTime = 0.5f;
    float timeCounter = 0.0f;
    // Use this for initialization
    void Start () {
        frameRate = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        timeCounter += Time.deltaTime;
        if (timeCounter > refreshTime)
        {
            frameRate.text = ((int)fps).ToString();
            timeCounter = 0.0f;
        }
	}
}
