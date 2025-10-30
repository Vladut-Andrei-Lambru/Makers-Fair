using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PlankRotationController : MonoBehaviour
{
    public InputActionReference joystickInput;
    public float rotationSpeed = 90f;
    public float deadzone = 0.2f;

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private PlankGroupGrabSync grabSync;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        grabSync = GetComponent<PlankGroupGrabSync>();
    }

    void Update()
    {
        if (!grab.isSelected || joystickInput == null || joystickInput.action == null) return;

        // Clear any residual angular velocity when grabbed
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }

        Vector2 input = joystickInput.action.ReadValue<Vector2>();
        if (input.magnitude < deadzone) return;

        float speed = rotationSpeed * Time.deltaTime;

        // LEFT/RIGHT → Rotate around Y axis (spin like a wheel)
        // UP/DOWN → Rotate around X axis (tilt forward/back)
        
        float yRotation = input.x * speed;  // Left/Right = spin
        float xRotation = -input.y * speed; // Up/Down = tilt (negative so up tilts forward)

        // Apply rotation
        if (grabSync != null && grabSync.IsLeader())
        {
            PlankGroup group = grabSync.GetGroup();
            if (group != null)
            {
                // Also clear velocities for all planks in the group
                foreach (var plankRb in group.planks)
                {
                    plankRb.angularVelocity = Vector3.zero;
                    plankRb.linearVelocity = Vector3.zero;
                }
                
                if (Mathf.Abs(yRotation) > 0.01f)
                    group.RotateGroupOnAxis(Vector3.up, yRotation);
                if (Mathf.Abs(xRotation) > 0.01f)
                    group.RotateGroupOnAxis(Vector3.right, xRotation);
                return;
            }
        }

        // Single plank
        if (Mathf.Abs(yRotation) > 0.01f)
            transform.Rotate(Vector3.up, yRotation, Space.Self);
        if (Mathf.Abs(xRotation) > 0.01f)
            transform.Rotate(Vector3.right, xRotation, Space.Self);
    }
}