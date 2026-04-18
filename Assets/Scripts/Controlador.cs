using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GravityReceiver))]
[RequireComponent(typeof(GroundDetector))]
[RequireComponent(typeof(PlayerController))]
public class Controlador : MonoBehaviour
{
    [Header("Legacy references")]
    public Rigidbody2D rb;

    [Header("Legacy movement values")]
    public float moveSpeed = 10f;
    public float salto = 10f;
    public float maxHorizontalSpeed = 10f;

    [Header("Legacy state")]
    public Vector2 gravityForce;
    public bool estaSuelo;

    private GravityReceiver gravityReceiver;
    private GroundDetector groundDetector;
    private PlayerController playerController;

    private void Awake()
    {
        rb = GetOrAddComponent<Rigidbody2D>();
        gravityReceiver = GetOrAddComponent<GravityReceiver>();
        groundDetector = GetOrAddComponent<GroundDetector>();
        playerController = GetOrAddComponent<PlayerController>();

        SyncToComponents();
    }

    private void Reset()
    {
        rb = GetOrAddComponent<Rigidbody2D>();
        gravityReceiver = GetOrAddComponent<GravityReceiver>();
        groundDetector = GetOrAddComponent<GroundDetector>();
        playerController = GetOrAddComponent<PlayerController>();

        SyncToComponents();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        SyncCachedReferences();
        SyncToComponents();
    }

    private void Update()
    {
        if (gravityReceiver != null)
        {
            gravityForce = gravityReceiver.GravityForce;
        }

        if (groundDetector != null)
        {
            estaSuelo = groundDetector.EstaSuelo;
        }
    }

    public void AddGravityForce(Vector2 force)
    {
        if (gravityReceiver == null)
        {
            gravityReceiver = GetComponent<GravityReceiver>();
        }

        gravityReceiver?.AddGravityForce(force);

        if (gravityReceiver != null)
        {
            gravityForce = gravityReceiver.GravityForce;
        }
    }

    private void SyncCachedReferences()
    {
        rb ??= GetComponent<Rigidbody2D>();
        gravityReceiver ??= GetComponent<GravityReceiver>();
        groundDetector ??= GetComponent<GroundDetector>();
        playerController ??= GetComponent<PlayerController>();
    }

    private void SyncToComponents()
    {
        SyncCachedReferences();

        if (playerController == null)
        {
            return;
        }

        playerController.rb = rb;
        playerController.salto = salto;
        playerController.maxHorizontalSpeed = maxHorizontalSpeed > 0f ? maxHorizontalSpeed : moveSpeed;
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }
}
