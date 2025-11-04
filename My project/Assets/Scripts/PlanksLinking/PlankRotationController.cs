using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PlankRotationController : MonoBehaviour
{
    [Header("Input")] 
    public InputActionReference joystickInput;
    public InputActionReference cycleAxisButton;

    [Header("Rotation Settings")] 
    public float rotationSpeed = 90f;
    public float deadzone = 0.2f;

    [Header("Visual Indicators")]
    public GameObject xAxisIcon; // Assign X axis 3D model/sprite
    public GameObject yAxisIcon; // Assign Y axis 3D model/sprite
    public GameObject zAxisIcon; // Assign Z axis 3D model/sprite
    public Vector3 iconOffset = new Vector3(0, 0.5f, 0); // Position relative to object
    public float iconScale = 0.2f;
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.gray;

    private XRGrabInteractable grab;
    private Rigidbody rb;
    private PlankGroupGrabSync grabSync;

    private int currentAxis = 0;
    private bool buttonWasPressed = false;

    private GameObject[] axisIcons;
    private Transform iconParent;
    private Camera playerCamera;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        grabSync = GetComponent<PlankGroupGrabSync>();
    }

    void Start()
    {
        // Create parent container for icons
        iconParent = new GameObject("RotationIcons").transform;
        iconParent.SetParent(transform);
        iconParent.localPosition = iconOffset;

        // Store references
        axisIcons = new GameObject[] { xAxisIcon, yAxisIcon, zAxisIcon };

        // Setup icons
        SetupIcons();
        
        // Find main camera (player's view)
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            // Try to find XR camera
            playerCamera = FindFirstObjectByType<Camera>();
        }
    }

    void SetupIcons()
    {
        for (int i = 0; i < axisIcons.Length; i++)
        {
            if (axisIcons[i] == null) continue;

            // Instantiate icon as child of parent
            GameObject icon = Instantiate(axisIcons[i], iconParent);
            icon.transform.localPosition = new Vector3((i - 1) * 0.3f, 0, 0); // Spread horizontally
            icon.transform.localScale = Vector3.one * iconScale;
            axisIcons[i] = icon;

            // Set initial color
            UpdateIconColor(icon, i == currentAxis);
        }

        // Hide icons initially
        iconParent.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (cycleAxisButton != null && cycleAxisButton.action != null)
            cycleAxisButton.action.Enable();
    }

    void Update()
    {
        // Show icons only when grabbed
        bool isGrabbed = grab.isSelected;
        if (iconParent != null)
        {
            iconParent.gameObject.SetActive(isGrabbed);
            
            // Make icons face the player
            if (isGrabbed && playerCamera != null)
            {
                iconParent.rotation = Quaternion.LookRotation(iconParent.position - playerCamera.transform.position);
            }
        }

        if (!isGrabbed || joystickInput == null || joystickInput.action == null) return;

        // Clear velocities
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }

        // Handle axis cycling
        if (cycleAxisButton != null && cycleAxisButton.action != null)
        {
            bool buttonPressed = cycleAxisButton.action.ReadValue<float>() > 0.5f;

            if (buttonPressed && !buttonWasPressed)
            {
                int oldAxis = currentAxis;
                currentAxis = (currentAxis + 1) % 3;
                
                // Update visual indicators
                UpdateIconColor(axisIcons[oldAxis], false);
                UpdateIconColor(axisIcons[currentAxis], true);
                
                Debug.Log($"[Rotation] Now rotating on axis: {GetAxisName()}");
            }

            buttonWasPressed = buttonPressed;
        }

        // Read joystick and apply rotation
        Vector2 input = joystickInput.action.ReadValue<Vector2>();
        if (input.magnitude < deadzone) return;

        float speed = rotationSpeed * Time.deltaTime;
        float rotationAmount = Mathf.Abs(input.x) > Mathf.Abs(input.y) ? input.x * speed : input.y * speed;
        Vector3 rotationAxis = GetCurrentAxis();

        // Apply rotation
        if (grabSync != null && grabSync.IsLeader())
        {
            PlankGroup group = grabSync.GetGroup();
            if (group != null)
            {
                foreach (var plankRb in group.planks)
                {
                    plankRb.angularVelocity = Vector3.zero;
                    plankRb.linearVelocity = Vector3.zero;
                }

                group.RotateGroupOnAxis(rotationAxis, rotationAmount);
                return;
            }
        }

        transform.Rotate(rotationAxis, rotationAmount, Space.Self);
    }

    void UpdateIconColor(GameObject icon, bool isActive)
    {
        if (icon == null) return;

        Color color = isActive ? activeColor : inactiveColor;

        // Try to set color on Renderer
        var renderer = icon.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        // Try to set color on SpriteRenderer
        var spriteRenderer = icon.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        // Scale active icon slightly larger
        icon.transform.localScale = Vector3.one * iconScale * (isActive ? 1.5f : 1f);
    }

    Vector3 GetCurrentAxis()
    {
        switch (currentAxis)
        {
            case 0: return Vector3.right;
            case 1: return Vector3.up;
            case 2: return Vector3.forward;
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

    void OnDestroy()
    {
        if (iconParent != null)
            Destroy(iconParent.gameObject);
    }
}