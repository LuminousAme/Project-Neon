using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionOnEscape : MonoBehaviour
{
    [SerializeField] SceneTransition sceneTransition;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && sceneTransition != null) sceneTransition.beginTransition(0);
    }
}
