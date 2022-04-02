using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BatColourManager : MonoBehaviour
{
    [SerializeField] MeshRenderer axeBlade;
    [SerializeField] MeshRenderer bat;
    [SerializeField] VisualEffect lightning, quickSwing, heavySwing;

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

        if(lightning != null)
        {
            lightning.SetVector4("LightingColor", color);
        }

        if(quickSwing != null)
        {
            quickSwing.SetVector4("SlashColour", color);
        }

        if (heavySwing != null)
        {
            heavySwing.SetVector4("SlashColour", color);
        }
    }
}
