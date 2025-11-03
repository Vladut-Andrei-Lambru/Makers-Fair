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
    private HashSet<Rigidbody> inTrigger = new();

    void Awake()
    {
        _rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        _rb.useGravity = true;
        _rb.isKinematic = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        if (hammerHead == null)
        {
            var hammerGO = GameObject.FindGameObjectWithTag("Hammer");
            if (hammerGO != null)
            {
                var rootCol = hammerGO.GetComponent<Collider>();
                if (rootCol != null)
                    hammerHead = rootCol;
                else
                {
                    hammerHead = hammerGO.GetComponentInChildren<Collider>();
                }
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
        if (++_hitCount >= hitsToLock)
            StartCoroutine(Weld());
        if (visual) visual.localPosition = Vector3.forward * (depthPerHit * _hitCount);
    }

    IEnumerator Weld()
    {
        if (_welded) yield break;
        _welded = true;

        if (grab && grab.isSelected && grab.interactionManager != null && grab.firstInteractorSelecting != null)
            grab.interactionManager.SelectExit(grab.firstInteractorSelecting, grab);

        Destroy(grab);
        Destroy(_rb);

        foreach (var col in GetComponents<Collider>())
            col.enabled = false;

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

        PlankGroup groupA = null;
        PlankGroup groupB = null;

        PlankGroup.plankToGroup.TryGetValue(rbA, out groupA);
        PlankGroup.plankToGroup.TryGetValue(rbB, out groupB);

        PlankGroup finalGroup;

        if (groupA != null && groupB != null && groupA != groupB)
        {
            finalGroup = groupA;
            finalGroup.MergeGroup(groupB);
        }
        else if (groupA != null)
        {
            finalGroup = groupA;
            finalGroup.AddPlank(rbB);
        }
        else if (groupB != null)
        {
            finalGroup = groupB;
            finalGroup.AddPlank(rbA);
        }
        else
        {
            finalGroup = PlankGroup.GetOrCreateGroup(rbA);
            finalGroup.AddPlank(rbB);
        }

        finalGroup.WeldAll();

        foreach (var rb in finalGroup.planks)
        {
            if (!rb.TryGetComponent(out PlankGroupGrabSync _))
                rb.gameObject.AddComponent<PlankGroupGrabSync>();

            // Enable socket children
            foreach (Transform child in rb.transform)
            {
                if (child.name.ToLower().Contains("socket"))
                {
                    child.gameObject.SetActive(true);
                    Debug.Log($"[Nail] Enabled socket: {child.name} on {rb.name}");
                }
            }
        }

        var fj = gameObject.AddComponent<FixedJoint>();
        fj.connectedBody = rbA;
        fj.breakForce = fj.breakTorque = Mathf.Infinity;

        // Notify WheelSocketHologram to refresh
        var wheelHologram = FindFirstObjectByType<WheelSocketHologram>();
        if (wheelHologram != null)
        {
            wheelHologram.OnPlanksJoined(finalGroup);
            Debug.Log("[Nail] Notified WheelSocketHologram to refresh sockets");
        }
    }
}