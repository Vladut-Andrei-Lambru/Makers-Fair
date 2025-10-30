using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
public class NailBehavior : MonoBehaviour
{
    [Header("Setup")] public Transform visual;
    public Transform nailTip;
    public Collider hammerHead;
    public XRGrabInteractable grab;
    public float depthPerHit = 0.02f;
    public int hitsToLock = 3;
    public LayerMask plankMask;

    private Rigidbody _rb;
    private int _hitCount;
    private bool _welded;
    private HashSet<Rigidbody> inTrigger = new();

    void Awake()
    {
        _rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        _rb.useGravity = true;
        _rb.isKinematic = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
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

        if (inTrigger.Count != 2)
        {
            Debug.Log("[Nail] Need exactly 2 planks in trigger");
            yield break;
        }

        var rbs = new List<Rigidbody>(inTrigger);
        var rbA = rbs[0];
        var rbB = rbs[1];

        Physics.IgnoreCollision(rbA.GetComponent<Collider>(), rbB.GetComponent<Collider>(), true);

        // NEW: Check if either plank is already in a group
        PlankGroup groupA = null;
        PlankGroup groupB = null;

        PlankGroup.plankToGroup.TryGetValue(rbA, out groupA);
        PlankGroup.plankToGroup.TryGetValue(rbB, out groupB);

        PlankGroup finalGroup;

        if (groupA != null && groupB != null && groupA != groupB)
        {
            // Both planks are in DIFFERENT groups - merge them
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
            finalGroup.AddPlank(rbB);
        }

        finalGroup.WeldAll();

        // add grab-sync to every plank
        foreach (var rb in finalGroup.planks)
            if (!rb.TryGetComponent(out PlankGroupGrabSync _))
                rb.gameObject.AddComponent<PlankGroupGrabSync>();

        // nail stays forever (no Rigidbody needed for FixedJoint)
        var fj = gameObject.AddComponent<FixedJoint>();
        fj.connectedBody = rbA;
        fj.breakForce = fj.breakTorque = Mathf.Infinity;
    }
}