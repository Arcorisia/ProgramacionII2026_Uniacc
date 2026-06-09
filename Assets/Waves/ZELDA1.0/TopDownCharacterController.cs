using UnityEngine;

public class TopDownCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("References")]
    [SerializeField] private Camera isoCamera;

    private Rigidbody _rb;
    private Vector3 _moveDir;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (isoCamera == null)
            isoCamera = Camera.main;

        // Evitar que la física tumbe al personaje
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        // Input en Update para no perder frames
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h == 0f && v == 0f)
        {
            _moveDir = Vector3.zero;
            return;
        }

        Vector3 camForward = isoCamera.transform.forward;
        Vector3 camRight   = isoCamera.transform.right;

        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        _moveDir = (camForward * v + camRight * h).normalized;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // Preservar velocidad Y (gravedad) y reemplazar solo XZ
        Vector3 targetVelocity = _moveDir * moveSpeed;
        targetVelocity.y = _rb.linearVelocity.y;

        _rb.linearVelocity = targetVelocity;
    }

    private void HandleRotation()
    {
        if (_moveDir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(_moveDir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.fixedDeltaTime
        );
    }

}
