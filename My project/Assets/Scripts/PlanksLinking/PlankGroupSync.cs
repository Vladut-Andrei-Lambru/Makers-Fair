using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public sealed class PlankGroupGrabSync : MonoBehaviour
{
    private Rigidbody _rb;
    private XRGrabInteractable _grab;
    private PlankGroup _group;
    private PlankGroupGrabSync _leader;
    private Vector3 _localPos;
    private Quaternion _localRot;

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _grab = GetComponent<XRGrabInteractable>();
        _grab.selectEntered.AddListener(OnGrab);
        _grab.selectExited .AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs _) 
    {
        if (PlankGroup.plankToGroup.TryGetValue(_rb, out _group))
            _group.SetKinematicAndFollow(this);
    }

    void OnRelease(SelectExitEventArgs _) 
    {
        if (_group != null) _group.ReleaseKinematic();
    }

    internal void MarkFollow(PlankGroupGrabSync lead)
    {
        _leader = lead;
        if (lead != null)
        {
            _localPos = lead.transform.InverseTransformPoint(transform.position);
            _localRot = Quaternion.Inverse(lead.transform.rotation) * transform.rotation;
        }
    }

    void FixedUpdate()
    {
        if (_leader != null && _rb != null && _rb.isKinematic)
        {
            Vector3 targetPos = _leader.transform.TransformPoint(_localPos);
            Quaternion targetRot = _leader.transform.rotation * _localRot;
            
            _rb.MovePosition(targetPos);
            _rb.MoveRotation(targetRot);
        }
    }
    
    // Public method to check if this plank is currently the leader
    public bool IsLeader()
    {
        return _leader == null && _grab.isSelected;
    }
    
    // Public method to get the group
    public PlankGroup GetGroup()
    {
        return _group;
    }
}