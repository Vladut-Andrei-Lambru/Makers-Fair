using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OpenOnGrab : MonoBehaviour
{
    [Header("Assign the hinge (your Hinge_Right transform)")]
    public Transform hingeRight;

    [Header("Angles (deg)")]
    public float closedAngle;
    public float openAngle = 140f;

    [Header("Motion")]
    public float speed = 360f;
    public Vector3 localAxis = new(0, 0, 1); 

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    float targetAngle, currentAngle;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        currentAngle = closedAngle;
        targetAngle = closedAngle;
    }

    void OnEnable()
    {
        if (grab == null) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        // XRIT 3.x: use AddListener / RemoveListener
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        if (grab == null) return;
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }
    void OnGrab(SelectEnterEventArgs _)
    {
        targetAngle = openAngle;
    }
    void OnRelease(SelectExitEventArgs _)
    {
        targetAngle = closedAngle;
    }

    void Update()
    {
        currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, speed * Time.deltaTime);
        if (hingeRight != null)
        {
            var q = Quaternion.AngleAxis(currentAngle, localAxis.normalized);
            hingeRight.localRotation = q;
        }
    }
}