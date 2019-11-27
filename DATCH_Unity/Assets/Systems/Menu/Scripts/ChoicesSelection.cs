using HoloToolkit.Examples.GazeRuler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoicesSelection : MonoBehaviour
{

    public InteractObj interactObj;
    public enum InteractObj
    {
        Line,
        Mesh,
        NA
    }
    public Color col;
    private Toggle toggle;
    private Button button;
    public Tool parentTool;

    public bool useCoroutine = true;
    // Use this for initialization
    void Start()
    {
        toggle = GetComponent<Toggle>();
        button = GetComponent<Button>();
        // We want to search for the tool if we don't explicitly define it in inspector
        if(parentTool == null)
            SearchForParentTool(transform.parent);

        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(UpdateSelection);
        }

        if (button != null)
        {
            button.onClick.AddListener(SetParentToolSettings);
        }

    }

    public void UpdateSelection(bool value)
    {
        if (value)
        {
            switch (interactObj)
            {
                case InteractObj.Line:
                    SetLineColor(col);
                    break;
                case InteractObj.Mesh:
                    SetMeshColor(col);
                    break;
                case InteractObj.NA:
                    break;
                default:
                    break;
            }
            SetParentToolSettings();
        }
    }

    public void SetLineColor(Color col)
    {
        Renderer lineRend = MeasureManager.Instance.LinePrefab.GetComponent<Renderer>();
        lineRend.sharedMaterial.SetColor("_Color", col);
        Renderer pointRend = MeasureManager.Instance.PointPrefab.GetComponent<Renderer>();
        pointRend.sharedMaterial.SetColor("_Color", col);


        MeasureManager.Instance.defaultColor = col;
    }

    public void SetMeshColor(Color col)
    {
        MeasureManager.Instance.meshColor = col;
    }

    private void SetParentToolSettings()
    {
        if (useCoroutine)
        {
            StartCoroutine(WaitToSetParentSettings());
        }
        else
        {
            parentTool.ToggleGroupSettings();
        }
    }

    IEnumerator WaitToSetParentSettings()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        parentTool.ToggleGroupSettings();
    }


    private void SearchForParentTool(Transform currentParent)
    {
        if (currentParent != null)
        {
            if (currentParent.GetComponent<Tool>() != null)
            {
                parentTool = currentParent.GetComponent<Tool>();
            }

            if (parentTool == null)
                SearchForParentTool(currentParent.parent);
        }
    }
}
