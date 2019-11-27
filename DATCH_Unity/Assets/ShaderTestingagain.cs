 using UnityEngine;
 using UnityEngine.UI;
 // Will be used for testing UI later.
 //[ExecuteInEditMode]
 public class ShaderTestingagain : MonoBehaviour {
 
     public UnityEngine.Rendering.CompareFunction comparison = UnityEngine.Rendering.CompareFunction.Always;
 
     public bool apply = true;
 
     private void Update()
     {
         if (apply)
         {
             apply = false;
             Debug.Log("Updated material val");
            if (GetComponent<Image>() != null)
            {
                Image image = GetComponent<Image>();
                Material existingGlobalMat = image.materialForRendering;
                Material updatedMaterial = new Material(existingGlobalMat);
                updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);
                image.material = updatedMaterial;
            }
            if (GetComponent<Text>() != null)
            {
                Text text = GetComponent<Text>();
                Material existingGlobalMat = text.materialForRendering;
                Material updatedMaterial = new Material(existingGlobalMat);
                updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);
                text.material = updatedMaterial;
            }
            if (GetComponent<MeshRenderer>() != null)
            {
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                Material existingGlobalMat = meshRenderer.material;
                Material updatedMaterial = new Material(existingGlobalMat);
                updatedMaterial.SetInt("unity_GUIZTestMode", (int)comparison);
                meshRenderer.material = updatedMaterial;
            }
        }
     }
 }