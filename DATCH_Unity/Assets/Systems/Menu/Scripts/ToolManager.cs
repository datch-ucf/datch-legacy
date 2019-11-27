// Jonathan Valderrama
// This Script will be used to call functions for a group of tools
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour {

    public List<Tool> tools = new List<Tool>();
    public List<GameObject> keyboardItems = new List<GameObject>();
    public static ToolManager Instance;
	// Use this for initialization
	void Start () {

        if (Instance == null)
        {
            Instance = this.GetComponent<ToolManager>();
        }
        else
        {
            Destroy(this);
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    // Turn off the submenus for the main tools
    public void TurnoffSubmenus()
    {
        foreach(Tool t in tools)
        {
            t.ToggleGroupSettings();
        }
    }

    // turn off the keyboard if you are not saving
    public void TurnoffKeyboard()
    {
        foreach(GameObject go in keyboardItems)
        {
            go.SetActive(false);
        }

    }
}
