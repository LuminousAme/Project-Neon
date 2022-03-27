using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyAttack : MonoBehaviour, IHitboxListener
{
    [SerializeField] private Hitbox hitbox;
    private int attackIndex;
    [SerializeField] private PlayerState player;
    [SerializeField] private int baseDamage = 25;
    private List<Collider> alreadyHitThisAttack = new List<Collider>();
    [SerializeField] private float standardAttackLenght = 1f, speedAdjustment = 1f;
    private float timeElapsed = 0f, timeToComplete = 1f;
    private bool attackActive = false, attackReleased = false;
    [SerializeField] private Animator weaponHandAnimator;

    //subscribe to the hitbox callback
    private void OnEnable()
    {
        attackIndex = hitbox.AddListener(this);
    }

    //unsubscribe from the hitbox callback
    private void OnDisable()
    {
        hitbox.RemoveListenerAtIndex(attackIndex);
    }

    public void HitRegistered(Collider collider)
    {
        if (attackActive && !alreadyHitThisAttack.Contains(collider))
        {
            alreadyHitThisAttack.Add(collider);
            Hurtbox hurtbox = collider.GetComponent<Hurtbox>();
            if (hurtbox != null) hurtbox.ProcessHit(player, baseDamage); //this func handles updating hp, damage dealt, and kills done by both players invovled

            //you'd also play any effect particle effects, animations or anything else that should happen when this attack hits someone 

            //but you can't tell if there's a kill here, however the PlayerState class has event callbacks onNewKill and onRespawn which fire off when 
            //a player gets a kill and dies respectively, both passing the playerState script so you can check the player invovled
        }
    }

    public void BeginAttack()
    {
        attackActive = true;
        attackReleased = false;
        weaponHandAnimator.SetTrigger("BeginHeavy");

        hitbox.shape = Hitbox.HitboxShape.BOX;
        hitbox.state = Hitbox.HitboxState.ACTIVE;
        hitbox.boxHalfSize = new Vector3(1f, 0.3f, 0.5f);
        hitbox.transform.localPosition = new Vector3(0f, 0.3f, -0.1f);

        alreadyHitThisAttack.Clear();
    }

    public void ReleaseAttack()
    {
        attackReleased = true;
        weaponHandAnimator.SetTrigger("EndHeavy");
        timeElapsed = 0f;

        hitbox.shape = Hitbox.HitboxShape.BOX;
        hitbox.boxHalfSize = new Vector3(1f, 1.2f, 0.8f);
        hitbox.transform.localPosition = new Vector3(0f, -1f, 1.5f);
    }

    public void EndAttack()
    {
        attackActive = false;
        attackReleased = false;
        timeElapsed = 0f;

        hitbox.shape = Hitbox.HitboxShape.BOX;
        hitbox.state = Hitbox.HitboxState.OFF;
        hitbox.boxHalfSize = Vector3.zero;
        hitbox.transform.localPosition = Vector3.zero;

        alreadyHitThisAttack.Clear();
    }

    private void Start()
    {
        alreadyHitThisAttack.Clear();
        attackActive = false;
        attackReleased = false;
        timeElapsed = 0f;
        timeToComplete = standardAttackLenght / speedAdjustment;
    }

    private void Update()
    {
        if (attackReleased)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > timeToComplete) EndAttack();
        }
    }

    public bool GetAttackActive() => attackActive;
    public bool GetAttackReleased() => attackReleased;
}
