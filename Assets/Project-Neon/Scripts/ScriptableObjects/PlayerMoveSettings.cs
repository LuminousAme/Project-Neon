using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoveSettings", menuName = "ProjectNeon/Player/Movement")]
public class PlayerMoveSettings : ScriptableObject
{
    [Header("Floating Capsule Controls")]
    [SerializeField] private float rideHeight = 1f;
    public float GetRideHeight() => rideHeight;
    [SerializeField] private float rideSpringStr = 160f;
    public float GetSpringStr() => rideSpringStr;
    [SerializeField] private float rideSpringDamp = 30f;
    public float GetSpringDamp() => rideSpringDamp;
    [SerializeField] private LayerMask walkableMask;
    public LayerMask GetWalkableMask() => walkableMask;

    [Space]
    [Header("Rotation Correction Controls")]
    [SerializeField] private float rotationSpringDamp = 4f;
    public float GetRotationSpringDamp() => rotationSpringDamp;

    [Space]
    [Header("Running Controls")]
    [SerializeField] private float baseMaxSpeed = 8f;
    public float GetBaseMaxSpeed() => baseMaxSpeed;
    [SerializeField] private float baseAcceleration = 200f;
    public float GetBaseAcceleration() => baseAcceleration;
    [SerializeField] private float baseMaxAccelForce = 150f;
    public float GetBaseMaxAccelForce() => baseMaxAccelForce;

    [Space]
    [Header("Jump Controls")]
    [SerializeField] private float baseJumpHeight = 2f;
    public float GetBaseJumpHeight() => baseJumpHeight;
    [SerializeField] private float horiDistanceToPeak = 2f;
    public float GetHoriDistanceToPeak() => horiDistanceToPeak;
    [SerializeField] private float horiDistanceWhileFalling = 2f;
    public float GetHoriDistanceWhileFalling() => horiDistanceWhileFalling;
    [SerializeField] private uint airjumps = 1;
    public uint GetAirJumps() => airjumps;
    [SerializeField] private float coyoteTime = 0.35f;
    public float GetCoyoteTime() => coyoteTime;
    private float jumpInitialVerticalVelo;
    public float GetJumpInitialVerticalVelo() => jumpInitialVerticalVelo;
    private float gravityGoingUp;
    public float GetGravityGoingUp() => gravityGoingUp;
    private float gravityGoingDown;
    public float GetGravityGoingDown() => gravityGoingDown;

    [Space]
    [Header("Camera Controls")]
    [SerializeField] private float horizontalLookSpeed = 100f;
    public float GetHorizontalLookSpeed() => horizontalLookSpeed;
    [SerializeField] private float verticalLookSpeed = 100f;
    public float GetVerticalLookSpeed() => verticalLookSpeed;
    [SerializeField] private float vertMaxAngle = 80f;
    public float GetVertMaxAngle() => vertMaxAngle;
    [SerializeField] private float vertMinAngle = -80f;
    public float GetVertMinAngle() => vertMinAngle;

    private void OnEnable()
    {
        CalcJumpInfo();
    }

    private void OnValidate()
    {
        CalcJumpInfo();
    }

    private void CalcJumpInfo()
    {
        //jump calculations based on the building a better jump GDC talk, source: https://youtu.be/hG9SzQxaCm8
        jumpInitialVerticalVelo = (2f * baseJumpHeight * baseMaxSpeed) / horiDistanceToPeak;
        //calculate the gravity using the same variables (note two different gravities to allow for enhanced game feel)
        gravityGoingUp = (-2f * baseJumpHeight * (baseMaxSpeed * baseMaxSpeed) / (horiDistanceToPeak * horiDistanceToPeak));
        gravityGoingDown = (-2f * baseJumpHeight * (baseMaxSpeed * baseMaxSpeed) / (horiDistanceWhileFalling * horiDistanceWhileFalling));
    }
}
