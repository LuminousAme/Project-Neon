using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAttack : MonoBehaviour
{
    HeavyAttack heavyAttack;
    [SerializeField] LeanTweenHelper left;
    [SerializeField] LeanTweenHelper right;

    private void OnEnable()
    {
        heavyAttack = this.GetComponent<HeavyAttack>();
        heavyAttack.BeginAttack();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            heavyAttack.ReleaseAttack();
            left.BeginAll();
            right.BeginAll();
        }
    }
}
