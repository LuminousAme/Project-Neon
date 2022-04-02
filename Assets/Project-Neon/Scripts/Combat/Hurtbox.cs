using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Hit and hurt box system based on this game developer article https://www.gamedeveloper.com/design/hitboxes-and-hurtboxes-in-unity
//a hurtbox requires a collider of some type, however I do not trust requirecomponent to handle inherietence probably so I'm not putting it
[ExecuteAlways]
public class Hurtbox : MonoBehaviour
{
    //the player that is owning of this hurtbox
    [SerializeField] private PlayerState player;

    public enum HurtBoxShape
    {
        NONE,
        BOX,
        SPHERE,
        CAPSULE
    }
    private HurtBoxShape shape = HurtBoxShape.NONE;

    [Space]
    [Header("Gizmo Controls")]
    public bool drawGizmoOnSelectedOnly = false;
    public Color GizmoColor = Color.blue;

    private Vector3 halfBoxSize;
    private float sphereCapsuleRadius;
    private float capsuleHeight;

    private Mesh capsuleMeshForGizmo = null;

    public Action<PlayerState, int> onHurt;

    public void ProcessHit(PlayerState attackingPlayer, int damage, Vector3 hitPos)
    {
        int cappedDamage;
        bool killed = player.TakeDamage(damage, out cappedDamage, hitPos);
        attackingPlayer.DealDamage(cappedDamage, killed);
        onHurt?.Invoke(attackingPlayer, cappedDamage);
    }

    private void Start()
    {
        Setup();
    }

    private void OnValidate()
    {
        Setup();
    }

    void Setup()
    {
        if (GetComponent<BoxCollider>() != null)
        {
            BoxCollider bc = GetComponent<BoxCollider>();
            shape = HurtBoxShape.BOX;
            halfBoxSize = bc.bounds.extents;
        }
        else if (GetComponent<SphereCollider>() != null)
        {
            SphereCollider sc = GetComponent<SphereCollider>();
            shape = HurtBoxShape.SPHERE;
            sphereCapsuleRadius = sc.radius;
        }
        else if (GetComponent<CapsuleCollider>())
        {
            CapsuleCollider cc = GetComponent<CapsuleCollider>();
            shape = HurtBoxShape.CAPSULE;
            sphereCapsuleRadius = cc.radius;
            capsuleHeight = cc.height;
        }

        if(capsuleMeshForGizmo == null)
        {
            try
            {
                capsuleMeshForGizmo = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
            }
            catch
            {
                capsuleMeshForGizmo = null;
                Debug.Log("Could not load capsule mesh for hitbox gizmos");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (drawGizmoOnSelectedOnly) return;

        GizmoDrawing();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmoOnSelectedOnly) return;

        GizmoDrawing();
    }

    private void GizmoDrawing()
    {
        Gizmos.color = GizmoColor;

        Matrix4x4 temp;

        switch (shape)
        {
            case HurtBoxShape.BOX:
                temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, halfBoxSize * 2.0f);
                Gizmos.matrix = temp;
                break;

            case HurtBoxShape.SPHERE:
                temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawSphere(Vector3.zero, sphereCapsuleRadius);
                Gizmos.matrix = temp;
                break;

            case HurtBoxShape.CAPSULE:
                if (capsuleMeshForGizmo != null)
                {
                    Gizmos.DrawMesh(capsuleMeshForGizmo, transform.position, transform.rotation,
                        new Vector3(sphereCapsuleRadius, capsuleHeight * 0.25f, sphereCapsuleRadius));
                }
                break;
        }

        Gizmos.color = Color.white;
    }
}