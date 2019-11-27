using HoloToolkit.Examples.GazeRuler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerationManager : MonoBehaviour
{

    public static MeshGenerationManager Instance;
    public Material meshMaterial;
    private MeshGeneration meshGeneration;
    private List<GameObject> meshes = new List<GameObject>();
    private List<GameObject> savedMeshes = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        if (Instance == null)
        {
            Instance = this.GetComponent<MeshGenerationManager>();
        }
        else
        {
            Destroy(this);
        }        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject SetUpMeshObj(List<Vector3> verts)
    {
        GameObject meshObj = new GameObject("Mesh");
        meshObj.tag = "GeneratedMesh";
        meshObj.layer = 10; // GeneratedMesh
        meshGeneration = meshObj.AddComponent<MeshGeneration>();
        meshObj.GetComponent<MeshRenderer>().material = new Material(meshMaterial);
        meshes.Add(meshObj);

        Interact interact = meshObj.AddComponent<Interact>();
        interact.interactObj = Interact.InteractObj.Mesh;

        // Some type of highlight for mesh?
        //Material highlightMeshMat = Resources.Load("HighlightMeshObject") as Material;
        // just to set high light to null. change later
        interact.SetHighLightMaterial(null);


        GenerateNewMesh(verts, meshGeneration);
        ChangeMeshColor(meshObj, MeasureManager.Instance.meshColor);
        return meshObj;
    }

    private void GenerateNewMesh(List<Vector3> verts, MeshGeneration meshGeneration)
    {
        meshGeneration.DrawMesh(verts);
    }

    public List<GameObject> GetMeshes()
    {
        return meshes;
    }

    public void LoadSavedMesh(List<Vector3> verts)
    {
        GameObject meshObj = new GameObject("Mesh");
        meshObj.tag = "GeneratedMesh";
        meshObj.layer = 10; // GeneratedMesh
        meshGeneration = meshObj.AddComponent<MeshGeneration>();
        meshObj.GetComponent<MeshRenderer>().material = new Material(meshMaterial);
        savedMeshes.Add(meshObj);

        //Interact interact = meshObj.AddComponent<Interact>();
        //interact.interactObj = Interact.InteractObj.Mesh;

        GenerateNewMesh(verts, meshGeneration);
    }

    // find the meshes to update
    public void UpdateMesh(List<Vector3> verts, GameObject mesh)
    {
        if (mesh == null)
            return;

        GameObject meshObjToUpdate = meshes.Find(m => m == mesh);
        meshGeneration = meshObjToUpdate.GetComponent<MeshGeneration>();
        GenerateNewMesh(verts, meshGeneration);
    }

    public void DeleteMesh(GameObject mesh)
    {
        if (mesh == null)
            return;

        GameObject meshObjToDelete = meshes.Find(m => m == mesh);
        meshes.Remove(meshObjToDelete);
        Destroy(meshObjToDelete);
    }

    public void ChangeMeshColor(GameObject mesh, Color color)
    {
        GameObject meshObjToUpdate = meshes.Find(m => m == mesh);
        meshGeneration = meshObjToUpdate.GetComponent<MeshGeneration>();
        meshGeneration.SetMeshColor(color);
    }

    public void ChangeMeshProperties(bool active)
    {

        //foreach(GameObject m in meshes)
        //{
        //    meshGeneration = m.GetComponent<MeshGeneration>();
        //    meshGeneration.SetMeshProperties(active);
        //}
        
    }

    public void DeleteAllMeshesInScene()
    {
       GameObject[] generatedMeshes =  GameObject.FindGameObjectsWithTag("GeneratedMesh");

        foreach(GameObject gM in generatedMeshes)
        {
            Destroy(gM);
        }

        meshes = new List<GameObject>();
    }
}
