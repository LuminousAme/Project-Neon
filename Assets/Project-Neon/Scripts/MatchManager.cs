using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    private void OnEnable()
    {
        if(!SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.LoadScene(5, LoadSceneMode.Additive);
    }

    private void OnDisable()
    {
        if (SceneManager.GetSceneByBuildIndex(5).isLoaded) SceneManager.UnloadSceneAsync(5);
    }
}
