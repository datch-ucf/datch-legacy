/*
 * Jonathan Valderrama
 * Used for choosing different tool types and assigning corresponding actions 
 */


using HoloToolkit.Examples.GazeRuler;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tool : MonoBehaviour, IInputClickHandler, IInputHandler
{
    public enum ButtonType
    {
        BrushSize,
        Color,
        Undo,
        Fill,
        ClearAll,
        Save,
        NewSave,
        Load,
        NA
    }
    public ButtonType type;


    public GameObject subMenu; // Palette is only used for Color
    [HideInInspector]
    public GameObject parentMenu;
    public Toggle objectToDisable;
    private Toggle toggle; // This will be the toggle this script is attached to, if it has a toggle on the gameobject.
    private MeasureManager measureManager;
    private StorageManager storageManager;
    // Use this for initialization
    void Start()
    {
        measureManager = FindObjectOfType<MeasureManager>();
        storageManager = FindObjectOfType<StorageManager>();

        toggle = GetComponent<Toggle>();
        if (toggle)
        {
            toggle.onValueChanged.AddListener(ToggleSubMenu);
        }

    }

    public void SetUpMenus()
    {
        if (subMenu != null)
        {
            subMenu.GetComponent<Tool>().parentMenu = this.gameObject;
        }
    }

    public void ButtonAction()
    {
        switch (type)
        {
            case ButtonType.BrushSize:
                break;
            case ButtonType.Color:
                break;
            case ButtonType.Undo:
                measureManager.Undo();
                break;
            case ButtonType.Fill:
                MeshGenerationManager.Instance.ChangeMeshProperties(toggle.isOn);
                break;
            case ButtonType.ClearAll:
                measureManager.ClearAll();
                break;
            case ButtonType.Save:
                if (storageManager.GetSaved())
                {
                    ToggleGroupSettings(GameObject.Find("Save").GetComponent<Toggle>());
                    ToggleSubMenu(false);
                }
                else
                {
                    ToggleSubMenu(true);
                }
                storageManager.PromptForSave();
                break;
            case ButtonType.NewSave:
                storageManager.NewSave();
                ToggleSubMenu(true);
                break;
            case ButtonType.Load:
                ToggleFilesView();
                break;
            case ButtonType.NA:

                break;
        }
    }

    public void ToggleSubMenu(bool value)
    {
        if (subMenu != null)
        {
            subMenu.SetActive(value);
        }
    }

    private void ToggleFilesView()
    {
        StorageManager.Instance.LoadFileNames();
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        ButtonAction();
        eventData.Use();
    }

    public void OnInputDown(InputEventData eventData)
    {

    }

    public void OnInputUp(InputEventData eventData)
    {

    }

    // Only pass a toggle parameter if you are changing a different toggle than 
    // itself.
    public void ToggleGroupSettings(Toggle t = null)
    {
        ToggleGroup toggleGroup;
        Toggle tempToggle;
        if (t == null)
            tempToggle = toggle;
        else
            tempToggle = t;

        toggleGroup = tempToggle.group;
        toggleGroup.allowSwitchOff = true;
        tempToggle.isOn = false;
        toggleGroup.allowSwitchOff = false;
    }
}
