using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    bool isJumping;
    bool isJumpingInAnimator;
    int isJumpingKey;
    public void SetIsJumping(bool isJumping) => this.isJumping = isJumping;
    bool onGround;
    bool onGroundInAnimator;
    int onGroundKey;
    public void SetOnGround(bool onGround) => this.onGround = onGround;
    bool isFalling;
    bool isFallingInAnimator;
    int isFallingKey;
    public void SetIsFalling(bool isFalling) => this.isFalling = isFalling;
    bool moving;
    bool movingInAnimator;
    int movingKey;
    public void SetMoving(bool moving) => this.moving = moving;

    [SerializeField] SoundEffect landingSFX;
    [SerializeField] AudioSource landingAudioSource;

    [SerializeField] Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        isJumping = false;
        isJumpingInAnimator = false;
        isJumpingKey = Animator.StringToHash("IsJumping");
        onGround = true;
        onGroundInAnimator = true;
        onGroundKey = Animator.StringToHash("IsGrounded");
        isFalling = false;
        isFallingInAnimator = false;
        isFallingKey = Animator.StringToHash("IsFalling");
        moving = false;
        movingInAnimator = false;
        movingKey = Animator.StringToHash("IsMoving");
    }

    // Update is called once per frame
    void Update()
    {
        if(isJumping != isJumpingInAnimator)
        {
            animator.SetBool(isJumpingKey, isJumping);
            isJumpingInAnimator = isJumping;
        }

        if(onGround != onGroundInAnimator)
        {
            animator.SetBool(onGroundKey, onGround);
            onGroundInAnimator = onGround;
            if (onGround && landingSFX != null && landingAudioSource != null)
            {
                landingSFX.Play(landingAudioSource);
            }
        }

        if(isFalling != isFallingInAnimator)
        {
            animator.SetBool(isFallingKey, isFalling);
            isFallingInAnimator = isFalling;

        }

        if(moving != movingInAnimator)
        {
            animator.SetBool(movingKey, moving);
            movingInAnimator = moving;
        }
    }
}