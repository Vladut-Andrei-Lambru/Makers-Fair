using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PlankRotationController : MonoBehaviour
{
    [Header("Input")] public InputActionReference joystickInput;
    public InputActionReference cycleAxisButton; // X button to cycle axes

    [Header("Rotation Settings")] public float rotationSpeed = 90f;
    public float deadzone = 0.2f;

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private PlankGroupGrabSync grabSync;

    // Axis cycling: 0=X, 1=Y, 2=Z
    private int currentAxis = 0;
    private bool buttonWasPressed = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        grabSync = GetComponent<PlankGroupGrabSync>();
    }

    void OnEnable()
    {
        if (cycleAxisButton != null && cycleAxisButton.action != null)
            cycleAxisButton.action.Enable();
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

        // Handle X button press to cycle axes
        if (cycleAxisButton != null && cycleAxisButton.action != null)
        {
            bool buttonPressed = cycleAxisButton.action.ReadValue<float>() > 0.5f;

            if (buttonPressed && !buttonWasPressed)
            {
                currentAxis = (currentAxis + 1) % 3; // Cycle: X -> Y -> Z -> X
                Debug.Log($"[Rotation] Now rotating on axis: {GetAxisName()}");
            }

            buttonWasPressed = buttonPressed;
        }

        // Read joystick input
        Vector2 input = joystickInput.action.ReadValue<Vector2>();
        if (input.magnitude < deadzone) return;

        float speed = rotationSpeed * Time.deltaTime;

        // Use primary joystick direction (whichever is stronger)
        float rotationAmount = Mathf.Abs(input.x) > Mathf.Abs(input.y) ? input.x * speed : input.y * speed;

        // Get rotation axis based on current mode
        Vector3 rotationAxis = GetCurrentAxis();

        // Apply rotation
        if (grabSync != null && grabSync.IsLeader())
        {
            PlankGroup group = grabSync.GetGroup();
            if (group != null)
            {
                // Clear velocities for all planks in the group
                foreach (var plankRb in group.planks)
                {
                    plankRb.angularVelocity = Vector3.zero;
                    plankRb.linearVelocity = Vector3.zero;
                }

                group.RotateGroupOnAxis(rotationAxis, rotationAmount);
                return;
            }
        }

        // Single plank
        transform.Rotate(rotationAxis, rotationAmount, Space.Self);
    }

    Vector3 GetCurrentAxis()
    {
        switch (currentAxis)
        {
            case 0: return Vector3.right; // X axis
            case 1: return Vector3.up; // Y axis
            case 2: return Vector3.forward; // Z axis
            default: return Vector3.right;
        }
    }

    string GetAxisName()
    {
        switch (currentAxis)
        {
            case 0: return "X";
            case 1: return "Y";
            case 2: return "Z";
            default: return "X";
        }
    }
}