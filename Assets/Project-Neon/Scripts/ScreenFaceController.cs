using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFaceController : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;

    public void UpdateTexture(Texture newText)
    {
        if(meshRenderer != null)
        {
            meshRenderer.material.SetTexture("_BaseColorMap", newText);
        }
    }

    public void ClearCurrentTexture()
    {
        UpdateTexture(Texture2D.whiteTexture);
    }
}
