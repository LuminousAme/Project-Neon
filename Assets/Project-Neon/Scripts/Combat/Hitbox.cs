using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Hit and hurt box system based on this game developer article https://www.gamedeveloper.com/design/hitboxes-and-hurtboxes-in-unity
public class Hitbox : MonoBehaviour
{
    //all of the fields for the hitbox class are going to be public so they can be easily modified by other scripts later
    [Header("Interaction, Shape, and State")]
    public LayerMask hittableLayers;
    
    public enum HitboxShape
    {
        BOX,
        SPHERE,
        CAPSULE
    }
    public HitboxShape shape = HitboxShape.BOX;

    public enum HitboxState
    {
        OFF,
        ACTIVE,
        COLLIDING
    }
    public HitboxState state = HitboxState.OFF;

    [Space]
    [Header("Size")]
    public Vector3 boxHalfSize = Vector3.one / 2.0f;
    public float sphereCapsuleRadius = 1f;
    public float capsuleHalfHeight = 1f;

    [Space]
    [Header("Gizmo Controls")]
    public bool drawGizmoOnSelectedOnly = false;
    public bool drawGizmoIfOff = true;

    public Color inactiveGizmoColor = Color.grey;
    public Color activeGizmoColor = Color.yellow;
    public Color collidingGizmoColor = Color.red;

    private List<IHitboxListener> listeners = new List<IHitboxListener>();

    private Mesh capsuleMeshForGizmo;

    // Start is called before the first frame update
    void Start()
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

    // Update is called once per frame
    void Update()
    {
        if (state == HitboxState.OFF) return;

        Collider[] colliders = new Collider[0];

        switch(shape)
        {
            case HitboxShape.BOX:
                colliders = Physics.OverlapBox(transform.position, boxHalfSize, transform.rotation, hittableLayers);
                break;
            case HitboxShape.SPHERE:
                colliders = Physics.OverlapSphere(transform.position, sphereCapsuleRadius, hittableLayers);
                break;
            case HitboxShape.CAPSULE:
                Vector3 distanceMod = transform.up * ((0.5f * capsuleHalfHeight) - sphereCapsuleRadius);
                Vector3 point0 = transform.position - distanceMod;
                Vector3 point1 = transform.position + distanceMod;
                colliders = Physics.OverlapCapsule(point0, point1, sphereCapsuleRadius, hittableLayers);
                break;
        }

        if(colliders.Length > 0)
        {
            state = HitboxState.COLLIDING;
            for(int i = 0; i < colliders.Length; i++)
            {
                for(int j = 0; j < listeners.Count; j++)
                {
                    listeners[j].HitRegistered(colliders[i]);
                }
            }
        }
        else
        {
            state = HitboxState.ACTIVE;
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
        if (state != HitboxState.OFF || drawGizmoIfOff)
        {
            switch (state)
            {
                case HitboxState.OFF:
                    Gizmos.color = inactiveGizmoColor;
                    break;
                case HitboxState.ACTIVE:
                    Gizmos.color = activeGizmoColor;
                    break;
                case HitboxState.COLLIDING:
                    Gizmos.color = collidingGizmoColor;
                    break;
            }


            Matrix4x4 temp;


            switch (shape)
            {
                case HitboxShape.BOX:
                    temp = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.DrawCube(Vector3.zero, boxHalfSize * 2.0f);
                    Gizmos.matrix = temp;
                    break;

                case HitboxShape.SPHERE:
                    temp = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                    Gizmos.DrawSphere(Vector3.zero, sphereCapsuleRadius);
                    Gizmos.matrix = temp;
                    break;

                case HitboxShape.CAPSULE:
                    if (capsuleMeshForGizmo != null)
                    {
                        Gizmos.DrawMesh(capsuleMeshForGizmo, transform.position, transform.rotation,
                            new Vector3(sphereCapsuleRadius, capsuleHalfHeight, sphereCapsuleRadius));
                    }
                    break;
            }

            Gizmos.color = Color.white;
        }
    }

    //adds a listener to the hitbox and returns it's index
    public int AddListener(IHitboxListener newListener)
    {
        listeners.Add(newListener);
        return listeners.Count - 1;
    }

    //removes a listener at a given index
    public void RemoveListenerAtIndex(int index)
    {
        if (index < 0 || index > listeners.Count - 1) return;
        listeners.RemoveAt(index);
    }
}

public interface IHitboxListener
{
    void HitRegistered(Collider colliderHit);
}