using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

    XRGrabInteractable _grab;
    float _targetAngle, _currentAngle;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _currentAngle = closedAngle;
        _targetAngle = closedAngle;
    }

    void OnEnable()
    {
        if (_grab == null) _grab = GetComponent<XRGrabInteractable>();
        // XRIT 3.x: use AddListener / RemoveListener
        _grab.selectEntered.AddListener(OnGrab);
        _grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        if (_grab == null) return;
        _grab.selectEntered.RemoveListener(OnGrab);
        _grab.selectExited.RemoveListener(OnRelease);
    }
    void OnGrab(SelectEnterEventArgs _)
    {
        _targetAngle = openAngle;
    }
    void OnRelease(SelectExitEventArgs _)
    {
        _targetAngle = closedAngle;
    }

    void Update()
    {
        _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, speed * Time.deltaTime);
        if (hingeRight != null)
        {
            var q = Quaternion.AngleAxis(_currentAngle, localAxis.normalized);
            hingeRight.localRotation = q;
        }
    }
}