using UnityEngine;


public class NailDrive : MonoBehaviour
{
    [Header("Setup")]
    public Transform shaft;                 // the mesh that slides in
    public Transform head;                  // collider at hammerable head
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;       // socket on Plank A (set at runtime)
    public float totalDepth = 0.03f;        // how deep the nail travels into wood
    public Rigidbody rb;

    [Header("Stages")]
    [Range(0,3)] public int stage = 0;      // 0,1,2,3
    public float hitIncrement = 0.011f;     // each good hit depth
    public float minHitSpeed = 1.2f;        // hammer relative speed threshold

    // runtime
    Transform holeTransform;                // where the socket is on Plank A
    Rigidbody plankA;
    Rigidbody plankB;                       // detected by trigger on Plank B join point
    float drivenDepth = 0f;                 // current depth 0..totalDepth
    bool isSocketed = false;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        if (!shaft) shaft = transform;
    }

    void OnEnable()
    {
        // if the nail starts already socketed, XR will fire events after play starts.
        // we’ll detect in SocketListener below.
    }

    public void BindSocket(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor s, Transform hole, Rigidbody hostPlank)
    {
        socket = s; holeTransform = hole; plankA = hostPlank;
        isSocketed = true;
        // lock orientation/position relative to the hole
        AlignToHole();
    }

    public void UnbindSocket()
    {
        socket = null; holeTransform = null; plankA = null; isSocketed = false;
    }

    void AlignToHole()
    {
        if (!holeTransform) return;
        // align nail head flush to hole, pointing into plank along hole forward
        transform.position = holeTransform.position;
        transform.rotation = holeTransform.rotation;
        ApplyVisualDepth();
    }

    void ApplyVisualDepth()
    {
        // Move the shaft along its local forward (tip direction) INTO the wood.
        // Assume shaft forward is +Z. Adjust if your model differs.
        Vector3 local = shaft.localPosition;
        local.z = -drivenDepth; // push in
        shaft.localPosition = local;
    }

    public void RegisterJoinCandidate(Rigidbody otherPlank) => plankB = otherPlank;
    public void ClearJoinCandidate(Rigidbody otherPlank) { if (plankB == otherPlank) plankB = null; }

    // Called by HammerHit on collision if speed/angle is valid
    public void DriveByHit(float relativeSpeed, Vector3 hitDir)
    {
        if (!isSocketed || plankA == null) return;
        if (relativeSpeed < minHitSpeed) return;

        // Optional: require the hammer to strike along nail axis (within angle)
        float axisAlign = Vector3.Dot(transform.forward, -hitDir.normalized);
        if (axisAlign < 0.6f) return; // too glancing

        drivenDepth = Mathf.Clamp(drivenDepth + hitIncrement, 0f, totalDepth);

        // update stage thresholds
        int newStage = 0;
        if (drivenDepth > 0.001f) newStage = 1;                     // barely
        if (drivenDepth > totalDepth * 0.5f) newStage = 2;           // halfway
        if (Mathf.Approximately(drivenDepth, totalDepth)) newStage = 3; // full

        if (newStage != stage)
        {
            stage = newStage;
            // You can trigger sounds/VFX here per stage
        }

        ApplyVisualDepth();

        if (stage == 3) TryConnectPlanks();
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
        transform.SetParent(plankA.transform, true);

        // (Optional) Disable grabbing on both planks or just leave them dynamic.
        // (Optional) Disable grabbing the nail:
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab) grab.enabled = false;
    }
}
