using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NailDrive : MonoBehaviour
{
    [Header("Setup")]
    public Transform shaft;                  // The mesh that slides in
    public Transform head;                   // Collider at hammerable head
    public XRSocketInteractor socket;        // Socket on Plank A (set at runtime)
    public float totalDepth = 0.03f;         // How deep the nail travels into wood
    public Rigidbody rb;

    [Header("Stages")]
    [Range(0, 3)] public int stage = 0;      // 0,1,2,3
    public float hitIncrement = 0.011f;      // Each good hit depth
    public float minHitSpeed = 1.2f;         // Hammer relative speed threshold

    // Runtime
    Transform holeTransform;                 // Where the socket is on Plank A
    Rigidbody plankA;                        // Host plank (socket plank)
    Rigidbody plankB;                        // Detected by trigger on Plank B join point
    float drivenDepth = 0f;                  // Current depth 0..totalDepth
    bool isSocketed = false;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        if (!shaft) shaft = transform;
    }

    public void BindSocket(XRSocketInteractor s, Transform hole, Rigidbody hostPlank)
    {
        socket = s;
        holeTransform = hole;
        plankA = hostPlank;
        isSocketed = true;

        // ✅ Parent nail directly to the plank now – no physics needed
        transform.SetParent(plankA.transform, true);

        // Disable rigidbody motion entirely
        rb.isKinematic = true;
        rb.useGravity = false;

        AlignToHole();
    }

    public void UnbindSocket()
    {
        socket = null;
        holeTransform = null;
        plankA = null;
        isSocketed = false;

        // Detach and re-enable physics
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    void AlignToHole()
    {
        if (!holeTransform) return;

        // Align nail head flush to hole, pointing into plank along hole forward
        transform.position = holeTransform.position;
        transform.rotation = holeTransform.rotation;
        ApplyVisualDepth();
    }

    void ApplyVisualDepth()
    {
        // Move the shaft along its local forward (tip direction) INTO the wood.
        // Assume shaft forward is +Z. Adjust if your model differs.
        Vector3 local = shaft.localPosition;
        local.y = -drivenDepth; // push in
        shaft.localPosition = local;
    }

    public void RegisterJoinCandidate(Rigidbody otherPlank) => plankB = otherPlank;
    public void ClearJoinCandidate(Rigidbody otherPlank)
    {
        if (plankB == otherPlank) plankB = null;
    }

    // Called by HammerHit on collision if speed/angle is valid
    public void DriveByHit(float relativeSpeed, Vector3 hitDir)
    {
        if (!isSocketed || plankA == null) return;
        if (relativeSpeed < minHitSpeed) return;

        // Require the hammer to strike roughly along nail axis
        float axisAlign = Vector3.Dot(transform.forward, -hitDir.normalized);
        if (axisAlign < 0.6f) return; // too glancing

        drivenDepth = Mathf.Clamp(drivenDepth + hitIncrement, 0f, totalDepth);

        // Update stage thresholds
        int newStage = 0;
        if (drivenDepth > 0.001f) newStage = 1;                     // barely
        if (drivenDepth > totalDepth * 0.5f) newStage = 2;           // halfway
        if (Mathf.Approximately(drivenDepth, totalDepth)) newStage = 3; // full

        if (newStage != stage)
        {
            stage = newStage;
            // TODO: trigger sound or VFX for each stage here
        }

        ApplyVisualDepth();

        if (stage == 3)
            TryConnectPlanks();
    }

    void TryConnectPlanks()
    {
        if (plankA == null || plankB == null) return;

        // Make one joint; attach it to one plank and connect to the other.
        FixedJoint joint = plankA.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = plankB;
        joint.enableCollision = false;

        // Nail is now “locked” in; make it kinematic and parent to plankA
        rb.isKinematic = true;
        rb.useGravity = false;
        transform.SetParent(plankA.transform, true);

        // Disable grabbing the nail now that it's fixed
        var grab = GetComponent<XRGrabInteractable>();
        if (grab) grab.enabled = false;
    }
}
