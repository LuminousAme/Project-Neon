using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatColourManager : MonoBehaviour
{
    [SerializeField] MeshRenderer axeBlade;
    [SerializeField] MeshRenderer bat;

    [SerializeField] bool overrideColor = false;
    [SerializeField] Color colorToOverrideWith;

    private void Start()
    {
        if(overrideColor)
        {
            ApplyColour(colorToOverrideWith);
        }
    }

    public void ApplyColour(Color color)
    {
        axeBlade.material.SetColor("EmissionColor", color);
        bat.materials[1].SetColor("EmissionColor", color);
    }
}
