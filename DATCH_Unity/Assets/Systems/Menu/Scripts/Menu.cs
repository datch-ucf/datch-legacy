/*
 * This will hold simple functionality for the menu 
 */

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;


using System.Collections;
using System.Net;
using System.Net.Sockets;

using System;
using System.Net.NetworkInformation;
using HoloToolkit.Unity.UX;
using HoloToolkit.Examples.GazeRuler;

public class Menu : MonoBehaviour
{


    public static Menu Instance;

    public GameObject canvas;
    public GameObject curve;
    KeywordRecognizer keywordRecognizer;

    public GameObject cube;

    public Tagalong tagalong;
    public TapToPlace tapToPlace;
    public Billboard billboard;
    public Collider menuCollider;
    public GameObject drawingPlane;
    public Image currentMode;
    public List<Sprite> modeSprites = new List<Sprite>();
    public List<Material> spatialMapMat = new List<Material>();

    public List<GameObject> cursorPrefabs = new List<GameObject>();
    private int cursorCounter = 0;
    [HideInInspector]
    public bool movingVertex = false;

    // Defines which function to call when a keyword is recognized
    delegate void KeywordAction(PhraseRecognizedEventArgs args);
    Dictionary<string, KeywordAction> keywordCollection = new Dictionary<string, KeywordAction>();


    // Use this for initialization
    void Start()
    {


        if (Instance == null)
        {
            Instance = this.GetComponent<Menu>();
        }
        else
        {
            Destroy(this);
        }

        Application.targetFrameRate = 60;

        currentMode.sprite = modeSprites[0];
        // Setup the phrases you want to use for voice commands
        keywordCollection.Add("Hide Menu", TurnOffMenu);
        keywordCollection.Add("Show Menu", TurnOnMenu);
        keywordCollection.Add("Menu Follow", SetFollow);
        keywordCollection.Add("Place Menu", SetPlace);

        // Figure out reset scene
        //keywordCollection.Add("Reset Scene", ResetScene);

        // Vertext Movement
        keywordCollection.Add("Move Point", MoveVertex);
        keywordCollection.Add("Place Point", DrawVertex);
        //keywordCollection.Add("Snap To Grid", VertexMovementMode);
        //keywordCollection.Add("Free Transform", VertexMovementMode);

        //keywordCollection.Add("Hide Curve", TurnOffCurve);
        //keywordCollection.Add("Show Curve", TurnOnCurve);

        // Drawing Plane
        keywordCollection.Add("Show Plane", ShowPlane);
        keywordCollection.Add("Hide Plane", HidePlane);

        keywordCollection.Add("Show Mesh", ShowMesh);
        keywordCollection.Add("Hide Mesh", HideMesh);
        //keywordCollection.Add("Switch Cursor", SwitchCursor);

        keywordCollection.Add("Update Shape", UpdateShape);

        keywordCollection.Add("Mesh", Mesh);

        keywordRecognizer = new KeywordRecognizer(keywordCollection.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();


    }

    // Update is called once per frame
    void Update()
    {
        #region Debugging
        //if (Input.GetKeyDown(KeyCode.F))
        //    SetFollow(new PhraseRecognizedEventArgs());
        //if (Input.GetKeyDown(KeyCode.P))
        //    SetPlace(new PhraseRecognizedEventArgs());

        if (Input.GetKeyDown(KeyCode.F))
            SpatialMappingManager.Instance.CleanupObserver();
        //if (Input.GetKeyDown(KeyCode.F))
        //    MoveVertex(new PhraseRecognizedEventArgs());
        //if (Input.GetKeyDown(KeyCode.P))
        //    DrawVertex(new PhraseRecognizedEventArgs());

        //if (Input.GetKeyDown(KeyCode.V))
        //    movingVertex = !movingVertex;
        //if (movingVertex)
        //{
        //    if (Input.GetKeyDown(KeyCode.S))
        //        SetVertexMovementMode("Snap To Grid");
        //    if (Input.GetKeyDown(KeyCode.F))
        //        SetVertexMovementMode("Free Transform");
        //}
        //else
        //{
        //    if (Input.GetKeyDown(KeyCode.P))
        //        SetVertexMovementMode();
        //}


        //if (Input.GetKeyDown(KeyCode.D))
        //    ShowPlane(new PhraseRecognizedEventArgs());
        //if (Input.GetKeyDown(KeyCode.S))
        //    HidePlane(new PhraseRecognizedEventArgs());
        //if (Input.GetKeyDown(KeyCode.S))
        //    SwitchCursor(new PhraseRecognizedEventArgs());
        //if (Input.GetKeyDown(KeyCode.R))
        //    ResetScene(new PhraseRecognizedEventArgs());

        //if (Input.GetKeyDown(KeyCode.P))
        //    DrawVertex(new PhraseRecognizedEventArgs());
        //if (Input.GetKeyDown(KeyCode.M))
        //    MoveVertex(new PhraseRecognizedEventArgs());
        #endregion
    }

    private void OnDestroy()
    {
        keywordRecognizer.Stop();
        keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Dispose();
    }
    private void ResetScene(PhraseRecognizedEventArgs args)
    {
        Scene loadedLevel = SceneManager.GetActiveScene();
        SceneManager.LoadScene(loadedLevel.buildIndex);
    }

    private void UpdateShape(PhraseRecognizedEventArgs args)
    {
        UpdateAllShapes();
    }

    private void Mesh(PhraseRecognizedEventArgs args)
    {
        SpatialMappingManager.Instance.SurfaceMaterial = spatialMapMat[2];
    }

    private void MoveVertex(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("MovePoint").GetComponent<Toggle>().isOn = true;
        //MoveVertex();
    }

    private void DrawVertex(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("PlacePoint").GetComponent<Toggle>().isOn = true;
        MeasureManager.Instance.SetDrawing(true, null);
        //DrawVertex();
    }

    private void VertexMovementMode(PhraseRecognizedEventArgs args)
    {
        if (movingVertex)
            SetVertexMovementMode("Free Transform");
    }

    public void DrawVertex()
    {
        movingVertex = false;
        currentMode.sprite = modeSprites[0];
        SetVertexMovementMode();
    }

    public void MoveVertex()
    {
        movingVertex = true;
        currentMode.sprite = modeSprites[1];
        // Everytime you go to move a vertex, the mode will be snap to grid
        // Change if necessary
        SetVertexMovementMode("Free Transform");
    }


    private void SetVertexMovementMode(string movementType = "")
    {
        GameObject[] allVertexinScene = GameObject.FindGameObjectsWithTag("Vertex");

        foreach (GameObject go in allVertexinScene)
        {
            switch (movementType)
            {
                case "Snap To Grid":
                    go.GetComponent<HandDraggable>().enabled = false;
                    go.GetComponent<TapToPlace>().enabled = true;
                    break;
                case "Free Transform":
                    go.GetComponent<TapToPlace>().enabled = false;
                    go.GetComponent<HandDraggable>().enabled = true;
                    break;
                default:
                    go.GetComponent<TapToPlace>().enabled = false;
                    go.GetComponent<HandDraggable>().enabled = false;
                    break;
            }
        }
    }

    // Listens to speech 
    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        KeywordAction keywordAction;

        if (keywordCollection.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke(args);
        }
    }

    // Turns off menu
    private void TurnOffMenu(PhraseRecognizedEventArgs args)
    {
        canvas.SetActive(false);
    }

    // Turns on menu
    private void TurnOnMenu(PhraseRecognizedEventArgs args)
    {
        canvas.SetActive(true);
    }

    // Turns on drawing plane
    private void ShowPlane(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("ShowPlane").GetComponent<Toggle>().isOn = true;
        //ShowPlane();   
    }

    // Turns off drawing plane
    private void HidePlane(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("ShowPlane").GetComponent<Toggle>().isOn = false;
        //HidePlane();
    }

    // Turns on Spatial Mesh
    private void ShowMesh(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("ShowMesh").GetComponent<Toggle>().isOn = true;

    }

    // Turns off Spatial Mesh
    private void HideMesh(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("ShowMesh").GetComponent<Toggle>().isOn = false;

    }

    // Turns off curve
    private void TurnOffCurve(PhraseRecognizedEventArgs args)
    {
        if (curve != null)
            curve.SetActive(false);
    }

    // Turns on curve
    private void TurnOnCurve(PhraseRecognizedEventArgs args)
    {
        if (curve != null)
            curve.SetActive(true);
    }

    private void SetFollow(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("MenuPlacement").GetComponent<Toggle>().isOn = false;
        //SetFollow();
    }

    private void SetPlace(PhraseRecognizedEventArgs args)
    {
        GameObject.Find("MenuPlacement").GetComponent<Toggle>().isOn = true;
        //SetPlace();
    }

    private void SetFollow()
    {
        tagalong.enabled = true;
        billboard.enabled = true;
        tapToPlace.enabled = false;
        cube.SetActive(false);
        menuCollider.enabled = false;

        if (WorldAnchorManager.Instance != null)
        {
            //Removes existing world anchor if any exist.
            WorldAnchorManager.Instance.RemoveAnchor(canvas);
        }
    }

    private void SetPlace()
    {
        tagalong.enabled = false;
        billboard.enabled = false;
        tapToPlace.enabled = true;
        cube.SetActive(true);
        menuCollider.enabled = true;

        if (WorldAnchorManager.Instance != null)
        {
            tapToPlace.IsBeingPlaced = false;
            //Removes existing world anchor if any exist.
            WorldAnchorManager.Instance.AttachAnchor(canvas);

        }
    }

    private void ShowPlane()
    {
        drawingPlane.SetActive(true);
        if (drawingPlane.GetComponent<BoundingBoxRig>().GetAppBarInsatnce() != null)
            drawingPlane.GetComponent<BoundingBoxRig>().GetAppBarInsatnce().gameObject.SetActive(true);

        drawingPlane.transform.position = GameObject.Find("DATCH_UI").transform.position;
        drawingPlane.transform.rotation = GameObject.Find("DATCH_UI").transform.rotation;
    }

    private void HidePlane()
    {
        drawingPlane.SetActive(false);
        if (drawingPlane.GetComponent<BoundingBoxRig>().GetAppBarInsatnce() != null)
        {
            drawingPlane.GetComponent<BoundingBoxRig>().GetAppBarInsatnce().ReturnToDefault();
            drawingPlane.GetComponent<BoundingBoxRig>().GetAppBarInsatnce().gameObject.SetActive(false);
        }
    }

    public void ToggleMenuState(bool value)
    {
        if (value)
        {
            SetPlace();
        }
        else
        {
            SetFollow();
        }
    }

    public void ToggleDrawingPlane(bool value)
    {
        if (value)
        {
            ShowPlane();
        }
        else
        {
            HidePlane();
        }
    }

    public void ToggleSpatialMesh(bool value)
    {
        // Figure out how to set up meshes
        //SpatialMappingManager.Instance.CleanupObserver();
        //SpatialMappingManager.Instance.StartObserver();

        if (value)
        {

            //SpatialMappingManager.Instance.SurfaceMaterial = spatialMapMat[0];
            SpatialMappingManager.Instance.SetSurfaceMaterial(spatialMapMat[0]);


        }
        else
        {

            //SpatialMappingManager.Instance.SurfaceMaterial = spatialMapMat[1];
            SpatialMappingManager.Instance.SetSurfaceMaterial(spatialMapMat[1]);

        }
    }

    private void SwitchCursor(PhraseRecognizedEventArgs args)
    {

        cursorPrefabs[cursorCounter].SetActive(false);
        cursorCounter++;

        if (cursorCounter >= cursorPrefabs.Count)
            cursorCounter = 0;

        cursorPrefabs[cursorCounter].SetActive(true);

    }

    // Here I want to change the material of cursor
    public void SwitchCuror(String cursor)
    {
        string cursorColor = "";


        foreach (GameObject g in cursorPrefabs)
        {
            g.SetActive(false);
        }
        switch (cursor)
        {
            case "White":
                cursorColor = "FFFFFFFF";
                cursorPrefabs[0].SetActive(true);
                break;
            case "Orange":
                cursorColor = "FFBF00FF";
                cursorPrefabs[1].SetActive(true);
                break;
            default:
                break;
        }


    }

    public void UpdateAllShapes()
    {
        List<HandDraggable> handDraggables = new List<HandDraggable>();
        handDraggables = GameObject.FindObjectsOfType<HandDraggable>().ToList();

        foreach (HandDraggable hd in handDraggables)
        {
            Interact interact = hd.GetComponent<Interact>();
            if (interact && PolygonManager.Instance)
            {
                if (interact.interactObj == Interact.InteractObj.Point)
                {
                    PolygonManager.Instance.UpdatePointPosition(hd.gameObject);
                    PolygonManager.Instance.UpdateLineLength();
                    PolygonManager.Instance.UpdatePolygonMesh();
                }
            }
        }
    }
}
