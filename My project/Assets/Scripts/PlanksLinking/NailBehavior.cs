using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
public class NailBehavior : MonoBehaviour
{
    [Header("Setup")]
    public Transform visual;
    public Transform nailTip;
    public Collider hammerHead;
    public XRGrabInteractable grab;
    public float depthPerHit = 0.02f;
    public int hitsToLock = 3;
    public LayerMask plankMask;

    private Rigidbody _rb;
    private int _hitCount;
    private bool _welded;
    private bool _inSocket;
    private HashSet<Rigidbody> inTrigger = new();
    private WheelSocket _currentSocket;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        _rb.useGravity = true;  // Normal gravity
        _rb.isKinematic = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        grab.selectExited.AddListener(_ => OnReleased());
    }

    void OnReleased()
    {
        // Check if nail is in a wheel socket
        CheckForSocket();
    }

    void CheckForSocket()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.05f);
        foreach (var hit in hits)
        {
            var socket = hit.GetComponent<WheelSocket>();
            if (socket != null && !socket.IsOccupied)
            {
                socket.AttachNail(this);
                _currentSocket = socket;
                _inSocket = true;
                
                // Snap nail into socket position
                _rb.isKinematic = true;
                transform.position = socket.transform.position;
                transform.rotation = socket.transform.rotation;
                
                Debug.Log("[Nail] Snapped into wheel socket");
                break;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & plankMask) == 0) return;
        var rb = other.attachedRigidbody;
        if (rb) inTrigger.Add(rb);
    }

    void OnTriggerExit(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (rb) inTrigger.Remove(rb);
    }

    void OnCollisionEnter(Collision c)
    {
        if (_welded || hammerHead == null) return;
        if (c.collider == hammerHead || c.collider.transform.IsChildOf(hammerHead.transform))
            Drive();
    }

    void Drive()
    {
        // If in socket, make it dynamic so it can be hammered
        if (_inSocket && _rb.isKinematic)
        {
            _rb.isKinematic = false;
            _rb.useGravity = false; // Don't fall while hammering
        }
        
        if (++_hitCount >= hitsToLock)
            StartCoroutine(Weld());
        if (visual) visual.localPosition = Vector3.forward * (depthPerHit * _hitCount);
    }

    IEnumerator Weld()
    {
        if (_welded) yield break;
        _welded = true;

        // drop grab if held
        if (grab && grab.isSelected && grab.interactionManager != null && grab.firstInteractorSelecting != null)
            grab.interactionManager.SelectExit(grab.firstInteractorSelecting, grab);

        // completely remove grab & physics from the nail
        Destroy(grab);
        Destroy(_rb);
        // nail becomes non-solid
        foreach (var col in GetComponents<Collider>())
            col.enabled = false;

        // push nail visually the last bit
        if (visual) visual.localPosition += Vector3.forward * depthPerHit * (hitsToLock - _hitCount);

        yield return new WaitForFixedUpdate();

        if (inTrigger.Count < 1 || inTrigger.Count > 2)
        {
            Debug.Log($"[Nail] Need 1-2 objects in trigger, found {inTrigger.Count}");
            yield break;
        }

        var rbs = new List<Rigidbody>(inTrigger);
        var rbA = rbs[0];
        Rigidbody rbB = rbs.Count > 1 ? rbs[1] : null;

        if (rbB != null)
        {
            Physics.IgnoreCollision(rbA.GetComponent<Collider>(), rbB.GetComponent<Collider>(), true);
        }

        // Check if either object is already in a group
        PlankGroup groupA = null;
        PlankGroup groupB = null;
        
        PlankGroup.plankToGroup.TryGetValue(rbA, out groupA);
        if (rbB != null)
            PlankGroup.plankToGroup.TryGetValue(rbB, out groupB);

        PlankGroup finalGroup;

        if (rbB == null)
        {
            // Single object (like attaching wheel to single plank)
            if (groupA != null)
            {
                finalGroup = groupA;
            }
            else
            {
                finalGroup = PlankGroup.GetOrCreateGroup(rbA);
            }
        }
        else if (groupA != null && groupB != null && groupA != groupB)
        {
            // Both objects are in DIFFERENT groups - merge them
            finalGroup = groupA;
            finalGroup.MergeGroup(groupB);
        }
        else if (groupA != null)
        {
            // Only A is in a group - add B to it
            finalGroup = groupA;
            finalGroup.AddPlank(rbB);
        }
        else if (groupB != null)
        {
            // Only B is in a group - add A to it
            finalGroup = groupB;
            finalGroup.AddPlank(rbA);
        }
        else
        {
            // Neither is in a group - create new group
            finalGroup = PlankGroup.GetOrCreateGroup(rbA);
            if (rbB != null)
                finalGroup.AddPlank(rbB);
        }

        finalGroup.WeldAll();

        // add grab-sync to every object in group
        foreach (var rb in finalGroup.planks)
            if (!rb.TryGetComponent(out PlankGroupGrabSync _))
                rb.gameObject.AddComponent<PlankGroupGrabSync>();

        // Mark socket as permanently occupied
        if (_currentSocket != null)
            _currentSocket.MarkPermanent();

        // nail stays forever (no Rigidbody needed for FixedJoint)
        var fj = gameObject.AddComponent<FixedJoint>();
        fj.connectedBody = rbA;
        fj.breakForce = fj.breakTorque = Mathf.Infinity;
        
        Debug.Log($"[Nail] Welded {inTrigger.Count} objects into group with {finalGroup.planks.Count} total objects");
    }
}