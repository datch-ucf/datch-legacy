using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Events;
using HoloToolkit.Examples.GazeRuler;
using UnityEngine.UI;
using System;

public class LineEditing : MonoBehaviour, IFocusable, IInputClickHandler
{
    public Renderer rendererComponent;

    public MeasureManager measureManager;

    public GameObject currentColor;

    private Renderer rend;

    public static Color lineColor;

    public Toggle objectToDisable;

    public enum InteractObj
    {
        Line,
        Mesh
    }
    public InteractObj interactObj;

    [System.Serializable]
    public class PickedColorCallback : UnityEvent<Color> { }

    public PickedColorCallback OnGazedColor = new PickedColorCallback();
    public PickedColorCallback OnPickedColor = new PickedColorCallback();

    private bool gazing = false;

    private void Start()
    {
        lineColor = measureManager.defaultColor;
    }

    private void Update()
    {
        if (gazing == false) return;        
    }

    private void UpdatePickedColor(PickedColorCallback cb)
    {
        RaycastHit hit = GazeManager.Instance.HitInfo;
        GameObject hitObj = GazeManager.Instance.HitObject;
        
        if (hitObj.transform.tag != "Swatch") { return; }
        
        var texture = (Texture2D)rendererComponent.material.mainTexture;

        Vector2 pixelUV = hit.textureCoord;
        pixelUV.x *= texture.width;
        pixelUV.y *= texture.height;
        Color col = texture.GetPixel((int)pixelUV.x, (int)pixelUV.y);
        
        cb.Invoke(col);
        switch (interactObj)
        {
            case InteractObj.Line:
                SetLineColor(col);
                break;
            case InteractObj.Mesh:
                SetMeshColor(col);
                break;
            default:
                break;
        }        
    }

    public void SetLineColor(Color col)
    {
        Renderer lineRend = measureManager.LinePrefab.GetComponent<Renderer>();
        lineRend.sharedMaterial.SetColor("_Color", col);
        Renderer pointRend = measureManager.PointPrefab.GetComponent<Renderer>();
        pointRend.sharedMaterial.SetColor("_Color", col);
    }

    public void SetMeshColor(Color col)
    {
        measureManager.meshColor = col;
    }

    public void OnFocusEnter()
    {
        gazing = true;
    }

    public void OnFocusExit()
    {
        gazing = false;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        //UpdatePickedColor(OnPickedColor);
        eventData.Use();
        measureManager.SetDrawing(true,null);
    }
}
