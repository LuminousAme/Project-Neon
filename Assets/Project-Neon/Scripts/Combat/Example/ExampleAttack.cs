using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleAttack : MonoBehaviour, IHitboxListener
{
    //the hitbox for this attack, you can modify it's shape, size, and other details from here to conform it to the attacks or animations
    //multiple attacks can share the same hitbox object but each player should have their own unique hitbox
    [SerializeField] private Hitbox hitbox;
    private int attackIndex;
    //the player that is owning of this attack, every player should have their own instances of the attack scripts 
    [SerializeField] private PlayerState player;
    [SerializeField] private int damage = 25;

    //this mostly exists to show how you can interface with the hit and hurtbox system, but you'd also stuff like animation control,
    //among other things to this script, so you can cycle through all of the motions of an attack, you'd probably want a public function to 
    //start an attack, use that to set a flag that tells update to run through the attack, but yeah

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

    //this function will be called with the collider when the hitbox detects a collision (assuming this is an IHitboxListener and subcribed to the hitbox)
    public void HitRegistered(Collider collider)
    {
        Hurtbox hurtbox = collider.GetComponent<Hurtbox>();
        if (hurtbox != null) hurtbox.ProcessHit(player, damage, new Vector3(1000f, 1000f, 1000f)); //this func handles updating hp, damage dealt, and kills done by both players invovled

        //you'd also play any effect particle effects, animations or anything else that should happen when this attack hits someone 

        //but you can't tell if there's a kill here, however the PlayerState class has event callbacks onNewKill and onRespawn which fire off when 
        //a player gets a kill and dies respectively, both passing the playerState script so you can check the player invovled
    }
}