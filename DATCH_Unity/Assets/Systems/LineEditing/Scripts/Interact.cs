/*
 * This script is used for adding actions to drawn interactable objects. 
 * 
 */
using HoloToolkit.Examples.GazeRuler;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour, IFocusable, IInputClickHandler, IHoldHandler
{
    public enum InteractObj
    {
        Point,
        Line,
        Mesh
    }
    public InteractObj interactObj;

    private HighlightObject highlight;
    
    void Awake()
    {
        if (interactObj != InteractObj.Mesh)
            highlight = new HighlightObject(transform);
    }

    public void Highlight()
    {
        if (highlight != null)
            highlight.Highlight();
    }

    public void UnHighlight()
    {
        if (highlight != null)
            highlight.UnHighlight();
    }

    public void OnFocusEnter()
    {
        Highlight();
    }

    public void OnFocusExit()
    {
        UnHighlight();
    }


    public void InteractAction()
    {
        switch (interactObj)
        {
            case InteractObj.Point:
                if(!Menu.Instance.movingVertex)
                    PolygonManager.Instance.SetNewVertex(gameObject);
                break;
            case InteractObj.Line:
                print(name);
                break;
            case InteractObj.Mesh:
                MeshGenerationManager.Instance.ChangeMeshColor(gameObject, MeasureManager.Instance.meshColor);
                break;
        }
    }
    
    public void OnInputClicked(InputClickedEventData eventData)
    {
        InteractAction();
        eventData.Use();
    }

    public void OnHoldStarted(HoldEventData eventData)
    {
        eventData.Use();
    }

    public void OnHoldCompleted(HoldEventData eventData)
    {

    }

    public void OnHoldCanceled(HoldEventData eventData)
    {

    }

    public void SetHighLightMaterial(Material material)
    {
        highlight = null;
        //highlight = new HighlightObject(transform, material);
    }
}
