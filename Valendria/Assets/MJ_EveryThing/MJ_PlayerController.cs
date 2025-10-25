using TMPro;
using UnityEngine;

public class MJ_PlayerController : MonoBehaviour
{
    public static MJ_PlayerController Instance; void Awake() => Instance = this;

    [Header("Player attr")]
    [SerializeField]
    private float _speed = 5f;

    float horizontal, vertical;


    Rigidbody rb;
    private Camera _cam;

    private Vector3 camForward, camRight;
    private bool promptIsShowing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
    }

    void Update()
    {
        PlayerYawToCamAlign();
        Vector3 dir = UpdateInputAndDirection();

        // Apply constant horizontal velocity
        rb.linearVelocity = new Vector3(dir.x * _speed, rb.linearVelocity.y, dir.z * _speed);
    }
    Vector3 UpdateInputAndDirection()
    {
        // Get input
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Build movement direction relative to camera
        camForward = _cam.transform.forward;
        camRight = _cam.transform.right;

        // Flatten to horizontal plane
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dir = (camForward * vertical + camRight * horizontal).normalized;
        return dir;
    }

    void PlayerYawToCamAlign()
    {
        // Align player yaw with camera yaw
        transform.rotation = Quaternion.Euler(0f, _cam.transform.eulerAngles.y, 0f);
    }

}