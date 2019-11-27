using HoloToolkit.Examples.GazeRuler;
using HoloToolkit.UI.Keyboard;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class StorageManager : MonoBehaviour
{
    private bool draw = false;
    // done
    public void Save()
    {
        draw = false;
        List<GameObject> meshes = meshGenerationManager.GetMeshes();

        if (polygonFileName != "")
        {
            draw = SHPFile.Instance.CreateShpFile(polygonFileName, meshes);
            SHPFile.Instance.CreateDBF_File(polygonFileName, meshes);
            print("SAVED");
            saved = true;
        }

        
        keyboardInputField.gameObject.SetActive(false);
    }    

    // done
    public void Load()
    {
        if (polygonFileName == "" || currentlyLoaded == polygonFileName)
        {
            print("AlreadyLoaded or nothing selected");
            return;
        }
        currentlyLoaded = polygonFileName;
        // clear all polygons
        // Make sure everything is cleared before you startd loading
        measureManager.ClearAll();

        if (!SHPFile.Instance.FileExists(polygonFileName))
            return;
        meshGenerationManager.DeleteAllMeshesInScene();
        print("loading");
        List<List<Vector3>> polygons = new List<List<Vector3>>(SHPFile.Instance.GetPolygons(polygonFileName));

        foreach (List<Vector3> polygon in polygons)
        {
            List<Vector3> vertices = new List<Vector3>(polygon);
            print(vertices.Count);
            
            meshGenerationManager.LoadSavedMesh(vertices);
        }
    }

    // done
    public void LoadFileNames()
    {
        fileNames = new List<string>(SHPFile.Instance.GetFilenames());
        UpdateFilesPanel();
    }

    // done
    public void DeleteFileName(string fileName)
    {
        fileNames.Remove(fileName);
        SHPFile.Instance.DeleteFileName(fileName);
        //SaveFileNames();

        UpdateFilesPanel();
    }

    // done
    public void UpdateFilesPanel()
    {
        fileNames.Sort((a, b) => a.CompareTo(b));

        foreach (Transform t in filesContainer.transform)
        {
            Destroy(t.gameObject);
        }

        foreach (string fN in fileNames)
        {
            GameObject tempButton = Instantiate(filePrefab);
            tempButton.transform.SetParent(filesContainer.transform, false);
            tempButton.GetComponentInChildren<Text>().text = fN;

            Button buttonComp = tempButton.GetComponent<Button>();

            buttonComp.onClick.AddListener(delegate
            {
                polygonFileName = fN;                
            });
        }
    }

    public void LoadingCanceled()
    {
        polygonFileName = "";
    }

    // TODO: What is the need for this
    // If you want to create a new file you want to reset the poly text
    public void NewFile()
    {
        savePolyTxt = new List<string>();
        newSave = false;
        loaded = false;
    }

    // done
    public void NewSave()
    {
        tempSaved = saved;
        saved = false;
        prevPolygonPropertiesSave = polygonFileName;
        PromptForSave();
    }

    // done    
    public void PromptForSave()
    {
        if (!saved)
        {
            //keyBoard.SetActive(true);
            //keyboardInputField.gameObject.SetActive(true);
            keyboardInputField.text = "";
            
        }
        else
        {
            Save();
        }
    }

    // done
    public void SetSaved()
    {
        saved = tempSaved;
    }

    public bool GetSaved()
    {
        return saved;
    }

    // done
    // Keyboard is closed and you try to save
    public void Prompt_OnEnter(object sender, EventArgs e)
    {
        //MeasureManager.Instance.SetDrawing(false);
        Keyboard keyboard = (Keyboard)sender;
        //print(keyboard.InputField.text);
        FileName_OnTextUpdated(keyboard.InputField.text);
        if (polygonFileName == "")
            polygonFileName = prevPolygonPropertiesSave;
        // Should I return here?

        //print("Clicke enter and saving!!");
        keyboardInputField.gameObject.SetActive(false);
        //keyBoard.SetActive(false);
        tempSaved = false;
        Save();

    }

    // done
    public void FileName_OnTextUpdated(string newText)
    {
        //print("Text updated");
        newText = newText.Trim();
        polygonFileName = newText;

        //print(polygonFileName);
    }

    // Use this for initialization
    void Start()
    {
        if (Instance == null)
        {
            Instance = this.GetComponent<StorageManager>();
        }
        else
        {
            Destroy(this);
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        #region Debugging
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    Save();
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    Load();
        //}
        #endregion
    }


    //You get that error because there is no support for Stream.Close on Windows Store Apps.
    #region Varialbes

    public static StorageManager Instance;

    // List for txt files
    private List<string> savePolyTxt = new List<string>(); // Holding polygon properties that will be saved

    private string polygonFileName = "";
    private string prevPolygonPropertiesSave = ""; // This if for when you try to make a new save and then cancel out. You will still be able to save with previous name

    private List<string> fileNames = new List<string>();
    private bool newSave = true;

    // Used to check if you previously saved and you exit out of trying for a new save
    // then you can keep using save button
    private bool tempSaved = false;
    private bool saved = false;
    private bool loaded = false;
    private string currentlyLoaded = "";

    public GameObject keyBoard;
    public KeyboardInputField keyboardInputField;
    public GameObject filesContainer;
    public GameObject filePrefab;
    public PolygonManager polygonManager;
    public MeasureManager measureManager;

    public MeshGenerationManager meshGenerationManager;

    public TextMesh modeTip;
    #endregion
}
