using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    bool isJumping;
    int isJumpingKey;
    public void SetIsJumping(bool isJumping) => this.isJumping = isJumping;
    bool onGround;
    int onGroundKey;
    public void SetOnGround(bool onGround) => this.onGround = onGround;
    bool isFalling;
    int isFallingKey;
    public void SetIsFalling(bool isFalling) => this.isFalling = isFalling;
    bool moving;
    int movingKey;
    public void SetMoving(bool moving) => this.moving = moving;

    [SerializeField] SoundEffect landingSFX, walkingSFX;
    [SerializeField] AudioSource landingAudioSource, walkingAudioSource;

    [SerializeField] Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        isJumping = false;
        isJumpingKey = Animator.StringToHash("IsJumping");
        onGround = false;
        onGroundKey = Animator.StringToHash("IsGrounded");
        isFalling = false;
        isFallingKey = Animator.StringToHash("IsFalling");
        moving = false;
        movingKey = Animator.StringToHash("IsMoving");
    }

    // Update is called once per frame
    void Update()
    {
        if (isJumping != animator.GetBool(isJumpingKey))
        {
            animator.SetBool(isJumpingKey, isJumping);
        }

        if(onGround != animator.GetBool(onGroundKey))
        {
            animator.SetBool(onGroundKey, onGround);
            if (onGround && landingSFX != null && landingAudioSource != null)
            {
                landingSFX.Play(landingAudioSource);
            }
        }

        if(isFalling != animator.GetBool(isFallingKey))
        {
            animator.SetBool(isFallingKey, isFalling);
        }

        if(moving != animator.GetBool(movingKey))
        {
            animator.SetBool(movingKey, moving);
        }

        //special case
        if(onGround && !isJumping && !isFalling && animator.GetCurrentAnimatorStateInfo(0).IsName("Jump Launch"))
        {
            animator.SetBool(isFallingKey, true);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk/Run"))
        {
            walkingSFX.Play(walkingAudioSource);
        }
        else if (walkingAudioSource.isPlaying) walkingAudioSource.Stop();
    }
}