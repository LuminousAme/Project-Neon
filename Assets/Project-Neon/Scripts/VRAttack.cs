using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRAttack : MonoBehaviour
{
    [SerializeField] Transform attackHand, cam, attackTarget;
    [SerializeField] float lightDistance, heavyDistance;
    [SerializeField] float heavytimeleeway = 0.5f;

    BasicPlayerController controller;

    bool attacked = false;
    bool heavyPrepared = false;
    float timeSinceHeavyPrepared;

    private void Start()
    {
        controller = GetComponent<BasicPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 between = attackHand.position - cam.position;

        bool up = between.y > heavyDistance;

        if (!heavyPrepared)
        {
            timeSinceHeavyPrepared = 0f;
            if (up) heavyPrepared = true;
        }
        else if (!attacked)
        {
            if (!up) timeSinceHeavyPrepared += timeSinceHeavyPrepared += Time.deltaTime;
            if(timeSinceHeavyPrepared > heavytimeleeway) heavyPrepared = false;
        }

        Vector3 betweentarget = attackTarget.position - cam.position;
        Vector3 projected = Vector3.Project(betweentarget, cam.forward);

        if (projected.magnitude < lightDistance) attacked = false;
        else if (!attacked) Attack();
    }

    void Attack()
    {
        attacked = true;
        if(heavyPrepared)
        {
            Debug.Log("Heavy Attack");
            controller.VRHeavyAttack();
            heavyPrepared = false;
        }
        else
        {
            Debug.Log("Light Attack");
            controller.VRLightAttack();
        }
    }
}
