using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class MJ_ThirdPersonCam : MonoBehaviour
{
    [Header("Roation Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float pitchMin = -30f;
    [SerializeField] private float pitchMax = 60f;
    [SerializeField] private float followSmoothness = 10f;

    private float yaw;   // horizontal rotation
    private float pitch; // vertical rotation
    private Vector3 desiredPosition;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float minZoomDistance = 2f;
    [SerializeField] private float maxZoomDistance = 12f;

    private float targetZoomDistance;

    void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;

        Cursor.lockState = CursorLockMode.Confined;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        HandleRotation();
        HandleZoom();

        // Calculate the new position from offset (based on zoom)
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 zoomedOffset = rotation * offset.normalized * targetZoomDistance;

        desiredPosition = target.position + zoomedOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmoothness * Time.deltaTime);
        transform.LookAt(target.position);
    }

    void HandleRotation()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Update yaw and pitch
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01)
        {
            targetZoomDistance = Mathf.Lerp(targetZoomDistance, targetZoomDistance - scrollInput * zoomSpeed, Time.deltaTime * 10f);
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }
    }
}