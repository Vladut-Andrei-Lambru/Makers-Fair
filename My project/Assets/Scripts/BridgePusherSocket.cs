using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BridgePusherSocket : MonoBehaviour
{
    [Header("Socket Setup")]
    public Transform socketCenter;
    public float moveDuration = 1.0f;
    public float pushDistance = 2.0f;
    public float pushForce = 1000f; // Use force instead of kinematic movement

    [Header("Mass & Break Limit")]
    public float maxBridgeMass = 50f;
    public bool debugMassCheck = true;

    [Header("Break Effects")]
    public float breakExplosionForce = 500f;
    public float breakExplosionRadius = 5f;

    [Header("UI Feedback")]
    public GameObject tooHeavyWarningUI; // Assign a UI panel in inspector
    public float warningDisplayTime = 2f;
    public AudioClip tooHeavySound; // Optional warning sound
    private AudioSource audioSource;

    private readonly HashSet<Rigidbody> insideRbs = new();
    private readonly List<Rigidbody> activeGroup = new();
    private bool groupSnapped;
    private bool isAnimating;
    private PlankGroup currentPlankGroup;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && tooHeavySound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        if (tooHeavyWarningUI != null)
            tooHeavyWarningUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (rb) insideRbs.Add(rb);
    }

    void OnTriggerExit(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (rb) insideRbs.Remove(rb);

        if (groupSnapped && !IsAnyInside(activeGroup))
            ReleaseGroup();
    }

    void Update()
    {
        if (groupSnapped || isAnimating) return;
        if (insideRbs.Count == 0) return;

        // Find a PlankGroup via one rigidbody
        foreach (var rb in insideRbs)
        {
            if (rb == null) continue;

            if (PlankGroup.plankToGroup.TryGetValue(rb, out var g))
            {
                if (g != null && g.planks.Count > 0)
                {
                    currentPlankGroup = g;
                    CollectEntireBridge();
                    SnapGroup();
                    groupSnapped = true;
                    return;
                }
            }
        }
    }

    void CollectEntireBridge()
    {
        activeGroup.Clear();

        // Add all planks from the group
        activeGroup.AddRange(currentPlankGroup.planks);

        // Find ALL wheels AND milk glasses attached to ANY plank in this group
        foreach (var plank in currentPlankGroup.planks)
        {
            if (plank == null) continue;

            // Check all children for sockets
            foreach (Transform child in plank.transform)
            {
                if (child.name.ToLower().Contains("socket"))
                {
                    // Check if socket has a wheel or milk child
                    foreach (Transform socketChild in child)
                    {
                        var itemRb = socketChild.GetComponent<Rigidbody>();
                        if (itemRb != null && (socketChild.CompareTag("Wheel") || socketChild.CompareTag("Milk")))
                        {
                            if (!activeGroup.Contains(itemRb))
                            {
                                activeGroup.Add(itemRb);
                                Debug.Log($"[BridgePusher] Found item: {socketChild.name} (tag: {socketChild.tag})");
                            }
                        }
                    }
                }
            }
        }

        int wheelCount = 0;
        int milkCount = 0;
        foreach (var rb in activeGroup)
        {
            if (rb != null)
            {
                if (rb.CompareTag("Wheel")) wheelCount++;
                else if (rb.CompareTag("Milk")) milkCount++;
            }
        }

        Debug.Log($"[BridgePusher] Collected bridge: {currentPlankGroup.planks.Count} planks + {wheelCount} wheels + {milkCount} milk = {activeGroup.Count} total rigidbodies");
    }

    void SnapGroup()
    {
        Vector3 avg = Vector3.zero;
        int count = 0;
        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;
            avg += rb.worldCenterOfMass;
            count++;
        }
        if (count == 0) return;
        avg /= count;

        Vector3 delta = socketCenter.position - avg;

        // DON'T make them kinematic - just stop their velocities and move them
        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.transform.position += delta;
        }

        Debug.Log($"[BridgePusher] Snapped complete bridge with {activeGroup.Count} rigidbodies (kept dynamic).");
    }

    public void PushButton()
    {
        if (!groupSnapped || isAnimating) return;

        float totalMass = 0f;
        float plankMass = 0f;
        float wheelMass = 0f;

        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;
            
            float mass = rb.mass;
            totalMass += mass;

            if (rb.CompareTag("Wheel"))
                wheelMass += mass;
            else
                plankMass += mass;
        }

        if (debugMassCheck)
        {
            Debug.Log($"[BridgePusher] === MASS CHECK ===");
            Debug.Log($"[BridgePusher] Planks: {plankMass:F2} kg");
            Debug.Log($"[BridgePusher] Wheels: {wheelMass:F2} kg");
            Debug.Log($"[BridgePusher] TOTAL: {totalMass:F2} kg / Limit: {maxBridgeMass:F2} kg");
        }

        if (totalMass > maxBridgeMass)
        {
            Debug.LogWarning($"[BridgePusher] BRIDGE TOO HEAVY! {totalMass:F2} > {maxBridgeMass:F2} - BREAKING!");
            StartCoroutine(ShowTooHeavyWarning());
            StartCoroutine(BreakBridge());
            return;
        }

        StartCoroutine(PushGroup());
    }

    IEnumerator BreakBridge()
    {
        isAnimating = true;

        yield return new WaitForSeconds(0.5f); // Small delay to let warning show

        Vector3 explosionCenter = socketCenter.position;

        // Break all joints and explode
        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;

            // Destroy all joints (nails and wheel connections)
            var joints = rb.GetComponents<FixedJoint>();
            foreach (var joint in joints)
            {
                Destroy(joint);
            }

            // Ensure physics is enabled
            rb.isKinematic = false;
            rb.useGravity = true;

            // Add explosion force
            Vector3 explosionDir = (rb.worldCenterOfMass - explosionCenter).normalized;
            explosionDir.y = Mathf.Abs(explosionDir.y); // Push upward
            rb.AddForce(explosionDir * breakExplosionForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * breakExplosionForce * 0.5f, ForceMode.Impulse);

            // Re-enable wheel grabbing
            var grab = rb.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
            {
                grab.enabled = true;
            }
        }

        Debug.Log("[BridgePusher] BRIDGE DESTROYED!");

        // Clear the group data
        if (currentPlankGroup != null)
        {
            currentPlankGroup.DissolveGroup();
        }

        ReleaseGroup();
        isAnimating = false;

        yield return null;
    }

    IEnumerator PushGroup()
    {
        isAnimating = true;

        // Calculate push direction and target
        Vector3 pushDir = socketCenter.right.normalized;
        Vector3 startPos = GetAveragePosition();
        Vector3 targetPos = startPos + pushDir * pushDistance;
        
        float duration = moveDuration;
        float elapsed = 0f;

        Debug.Log($"[BridgePusher] Starting push from {startPos} to {targetPos} (distance: {pushDistance}m)");

        // Use physics forces instead of direct position manipulation
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Calculate current average position
            Vector3 currentPos = GetAveragePosition();
            
            // Calculate how far we need to move
            Vector3 desiredPos = Vector3.Lerp(startPos, targetPos, t);
            Vector3 neededMove = desiredPos - currentPos;
            
            // Apply force to each rigidbody to push towards target
            foreach (var rb in activeGroup)
            {
                if (rb == null) continue;
                
                // Apply force proportional to mass and needed movement
                Vector3 force = neededMove * (rb.mass * pushForce / duration);
                rb.AddForce(force * Time.fixedDeltaTime, ForceMode.Force);
                
                // Dampen unwanted rotation
                rb.angularVelocity *= 0.95f;
            }

            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate(); // Use fixed update for physics
        }

        Debug.Log($"[BridgePusher] Push complete - final position: {GetAveragePosition()}");

        ReleaseGroup();
        isAnimating = false;
    }

    Vector3 GetAveragePosition()
    {
        Vector3 avg = Vector3.zero;
        int count = 0;
        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;
            avg += rb.worldCenterOfMass;
            count++;
        }
        return count > 0 ? avg / count : socketCenter.position;
    }

    bool IsAnyInside(List<Rigidbody> list)
    {
        var col = GetComponent<Collider>();
        if (!col) return false;
        foreach (var rb in list)
            if (rb != null && col.bounds.Contains(rb.transform.position))
                return true;
        return false;
    }

    void ReleaseGroup()
    {
        groupSnapped = false;
        activeGroup.Clear();
        currentPlankGroup = null;
    }

    IEnumerator ShowTooHeavyWarning()
    {
        if (tooHeavyWarningUI != null)
        {
            tooHeavyWarningUI.SetActive(true);
        }

        if (audioSource != null && tooHeavySound != null)
        {
            audioSource.PlayOneShot(tooHeavySound);
        }

        yield return new WaitForSeconds(warningDisplayTime);

        if (tooHeavyWarningUI != null)
        {
            tooHeavyWarningUI.SetActive(false);
        }
    }
}