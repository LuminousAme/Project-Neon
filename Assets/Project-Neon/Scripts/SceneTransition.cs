using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] LeanTweenHelper transitionTween;
    UnityEngine.UI.Image transitionImage; 
    int indexToLoad = -1;

    private void OnEnable()
    {
        LeanTweenHelper.onTweenComplete += ProcessTweenFinish;
    }

    private void OnDisable()
    {
        LeanTweenHelper.onTweenComplete -= ProcessTweenFinish;
    }

    void ProcessTweenFinish(LeanTweenHelper tween, int index)
    {
        if (tween != transitionTween) return;

        if(index == 0)
        {
            transitionImage.maskable = false;
            transitionImage.raycastTarget = false;
        }
        else if (index == 1)
        {
            SceneManager.LoadScene(indexToLoad);
        }
    }

    void Start()
    {
        transitionImage = transitionTween.GetComponent<UnityEngine.UI.Image>();
        transitionTween.BeginTween(0);
        transitionImage.maskable = true;
        transitionImage.raycastTarget = true;
    }

    public void beginTransition(int sceneIndex)
    {
        indexToLoad = sceneIndex;
        transitionTween.BeginTween(1);
    }
}