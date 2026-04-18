using UnityEngine;
using System.Collections.Generic;

public class GroundDetector : MonoBehaviour
{
    [SerializeField, Range(-1f, 1f)] private float minGroundDot = 0.25f;

    public bool EstaSuelo { get; private set; }

    private readonly HashSet<int> groundedColliders = new HashSet<int>();
    private GravityReceiver gravityReceiver;

    private void Awake()
    {
        gravityReceiver = GetComponent<GravityReceiver>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateCollisionState(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateCollisionState(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundedColliders.Remove(collision.collider.GetInstanceID());
        EstaSuelo = groundedColliders.Count > 0;
    }

    private void UpdateCollisionState(Collision2D collision)
    {
        Vector2 gravityDir = gravityReceiver != null ? gravityReceiver.GetGravityDirection() : Vector2.down;
        if (gravityDir == Vector2.zero)
        {
            gravityDir = Vector2.down;
        }

        bool isGroundContact = false;
        Vector2 upDirection = -gravityDir;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Vector2.Dot(contact.normal, upDirection) >= minGroundDot)
            {
                isGroundContact = true;
                break;
            }
        }

        int colliderId = collision.collider.GetInstanceID();
        if (isGroundContact)
        {
            groundedColliders.Add(colliderId);
        }
        else
        {
            groundedColliders.Remove(colliderId);
        }

        EstaSuelo = groundedColliders.Count > 0;
    }
}
