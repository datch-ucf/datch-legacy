using UnityEngine;
[System.Serializable]
public class HighlightObject {

    [HideInInspector]
    public bool outLine = true, canHighlight = true;
    public Material[] ogMaterials, newMaterials;
    private Material highlightAll, highlightOutline, highlightCustom;
    private MeshRenderer meshRenderer;
    private bool highlighted = false;

    private Material customHighlight;

    public HighlightObject(Transform obj, Material custom = null)
    {
        highlightCustom = custom;

        Material highlightMat = Resources.Load("HighlightObject") as Material;
        
        highlightOutline = highlightMat;

        meshRenderer = obj.GetComponent<MeshRenderer>();
        if(meshRenderer != null)
        {
            ogMaterials = meshRenderer.materials;
            InitializeNewMaterials();
        }
    }

    public void Highlight()
    {
        if (!highlighted && canHighlight)
        {
            meshRenderer.materials = newMaterials;
            highlighted = true;
        }
    }

    public void UnHighlight()
    {
        if (highlighted && canHighlight)
        {
            meshRenderer.materials = ogMaterials;
            highlighted = false;
        }
    }

    private void InitializeNewMaterials()
    {
        int materialsCount = ogMaterials.Length;
        newMaterials = new Material[materialsCount + 1];
        for (int i = 0; i < materialsCount; i++)
        {
            Material mat = ogMaterials[i];
            newMaterials[i] = mat;
        }

        newMaterials[materialsCount] = highlightCustom != null ? highlightCustom : outLine ? highlightOutline : highlightAll;
        canHighlight = true;
    }
}
