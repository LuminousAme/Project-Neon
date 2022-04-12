using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionOnStart : MonoBehaviour
{
    [SerializeField] SceneTransition sceneTransition;
    // Start is called before the first frame update
    void Start()
    {
        if (sceneTransition != null) sceneTransition.beginTransition(0);
    }
}
