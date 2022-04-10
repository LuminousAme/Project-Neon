using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.VFX;


public class QuickAttack : MonoBehaviour, IHitboxListener
{
    [SerializeField] private Hitbox hitbox;
    private int attackIndex;
    [SerializeField] private PlayerState player;
    [SerializeField] private int baseDamage = 10;
    private List<Collider> alreadyHitThisAttack = new List<Collider>();
    [SerializeField] private float standardAttackLenght = 1f, speedAdjustment = 1.5f;
    private float timeElapsed = 0f, timeToComplete = 1f;
    private bool attackActive = false;
    [SerializeField] private Animator weaponHandAnimator;
    [SerializeField] private VisualEffect slash;
    [SerializeField] private GameObject hitParticlePrefab;
    [SerializeField] private Transform particleSpawnPoint;

    [SerializeField] private SoundEffect hitSFX;

    public static Action OnQuickAttack;

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
        if(attackActive && !alreadyHitThisAttack.Contains(collider))
        {
            alreadyHitThisAttack.Add(collider);
            Hurtbox hurtbox = collider.GetComponent<Hurtbox>();
            if (hurtbox != null) hurtbox.ProcessHit(player, baseDamage, particleSpawnPoint.position); //this func handles updating hp, damage dealt, and kills done by both players invovled

            //you'd also play any effect particle effects, animations or anything else that should happen when this attack hits someone 
            GameObject newParticle = Instantiate(hitParticlePrefab, particleSpawnPoint.position, particleSpawnPoint.localRotation);
            Destroy(newParticle, 0.5f);

            //play the hit sound effect
            if (hitSFX != null) hitSFX.Play();

            //but you can't tell if there's a kill here, however the PlayerState class has event callbacks onNewKill and onRespawn which fire off when 
            //a player gets a kill and dies respectively, both passing the playerState script so you can check the player invovled

        }
    }

    public void BeginAttack()
    {
        attackActive = true;
        weaponHandAnimator.SetTrigger("Quick Attack");
        if (AsyncClient.instance != null) AsyncClient.instance.SendDoQuickAttack();
        StartCoroutine(startSlash());
        //if (slash != null) slash.Play();
        OnQuickAttack?.Invoke();

        hitbox.shape = Hitbox.HitboxShape.BOX;
        hitbox.state = Hitbox.HitboxState.ACTIVE;
        hitbox.boxHalfSize = new Vector3(0.6f, 0.5f, 0.8f);
        hitbox.transform.localPosition = new Vector3(0f, -0.1f, 1.5f);
        alreadyHitThisAttack.Clear();
    }

    public void EndAttack()
    {
        attackActive = false;
        if (slash != null)
        {
            slash.Stop();
            slash.gameObject.SetActive(false);
        }
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
        timeElapsed = 0f;
        timeToComplete = standardAttackLenght / speedAdjustment;

        if (slash != null)
        {
            slash.gameObject.SetActive(false);
        }
    }

    IEnumerator startSlash()
    {
        yield return new WaitForSeconds(0.1f);
        if (slash != null)
        {
            slash.gameObject.SetActive(true);
            slash.Play();
        }
    }

    private void Update()
    {
        if(attackActive)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed > timeToComplete) EndAttack();
        }
    }
}
