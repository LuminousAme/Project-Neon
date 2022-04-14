using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStartWalkAnim : MonoBehaviour
{
    [SerializeField] Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        if (animator != null) animator.SetBool("IsMoving", true);
    }
}
