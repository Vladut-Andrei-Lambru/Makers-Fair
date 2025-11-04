using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class BridgePusherSocket : MonoBehaviour
{
    [Header("Win Settings")]
    public float delayBeforeSceneLoad = 2f;
    public int mainMenuSceneIndex = 0;

    [Header("Mass Limit")]
    public float maxBridgeMass = 50f;

    [Header("Push Settings")]
    public Transform socketCenter;
    public float moveDuration = 2.0f;
    public float pushForce = 5000f;

    [Header("Break Effects")]
    public float breakExplosionForce = 500f;

    private readonly HashSet<Rigidbody> insideRbs = new HashSet<Rigidbody>();
    private readonly List<Rigidbody> activeGroup = new List<Rigidbody>();
    private PlankGroup currentPlankGroup;
    private bool isAnimating;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
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
    }

    public void StartButton()
    {
        if (isAnimating) return;

        // Find PlankGroup in trigger
        currentPlankGroup = null;
        foreach (var rb in insideRbs)
        {
            if (rb == null) continue;
            if (PlankGroup.plankToGroup.TryGetValue(rb, out var g))
            {
                if (g != null && g.planks.Count > 0)
                {
                    currentPlankGroup = g;
                    break;
                }
            }
        }

        if (currentPlankGroup == null)
        {
            Debug.LogWarning("[BridgePusher] No vehicle in zone!");
            return;
        }

        // Collect all parts
        activeGroup.Clear();
        activeGroup.AddRange(currentPlankGroup.planks);

        foreach (var plank in currentPlankGroup.planks)
        {
            if (plank == null) continue;

            foreach (Transform child in plank.transform)
            {
                if (child.name.ToLower().Contains("socket"))
                {
                    foreach (Transform socketChild in child)
                    {
                        var wheelRb = socketChild.GetComponent<Rigidbody>();
                        if (wheelRb != null && socketChild.CompareTag("Wheel"))
                        {
                            if (!activeGroup.Contains(wheelRb))
                                activeGroup.Add(wheelRb);
                        }
                    }
                }
            }
        }

        // Calculate total mass
        float totalMass = 0f;
        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;
            totalMass += rb.mass;
        }

        Debug.Log($"[BridgePusher] === MASS CHECK ===");
        Debug.Log($"[BridgePusher] Vehicle mass: {totalMass:F2} kg");
        Debug.Log($"[BridgePusher] Limit: {maxBridgeMass:F2} kg");

        // Check if pass or fail
        if (totalMass > maxBridgeMass)
        {
            Debug.LogWarning($"[BridgePusher] ❌ FAIL - TOO HEAVY! {totalMass:F2} > {maxBridgeMass:F2}");
            StartCoroutine(BreakBridge());
        }
        else
        {
            Debug.Log($"[BridgePusher] ✓ PASS - Mass OK!");
            StartCoroutine(PushGroup());
        }
    }

    IEnumerator BreakBridge()
    {
        isAnimating = true;
        Vector3 explosionCenter = socketCenter.position;

        Debug.Log($"[BridgePusher] 💥 BREAKING {activeGroup.Count} objects!");

        // Break all joints
        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;
            var joints = rb.GetComponents<Joint>();
            foreach (var j in joints) Destroy(j);
        }

        yield return new WaitForFixedUpdate();

        // Explode
        foreach (var rb in activeGroup)
        {
            if (rb == null) continue;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 dir = (rb.worldCenterOfMass - explosionCenter).normalized;
            dir.y = Mathf.Max(0.3f, Mathf.Abs(dir.y));
            rb.AddForce(dir * breakExplosionForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * breakExplosionForce * 0.5f, ForceMode.Impulse);
        }

        if (currentPlankGroup != null)
            currentPlankGroup.DissolveGroup();

        activeGroup.Clear();
        currentPlankGroup = null;
        
        // Wait a bit then load main menu
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        
        // Save result before loading scene
        PlayerPrefs.SetString("GameResult", "LOST");
        PlayerPrefs.Save();
        
        Debug.Log($"[BridgePusher] Loading scene index: {mainMenuSceneIndex}");
        SceneManager.LoadScene(mainMenuSceneIndex);
        
        isAnimating = false;
    }

    IEnumerator PushGroup()
    {
        isAnimating = true;

        Vector3 pushDir = socketCenter.right.normalized;
        Debug.Log($"[BridgePusher] 🚀 Pushing vehicle along X axis");

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            foreach (var rb in activeGroup)
            {
                if (rb == null) continue;
                rb.AddForce(pushDir * pushForce * Time.fixedDeltaTime, ForceMode.Force);
            }

            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log("[BridgePusher] ✓ Push complete!");
        
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        PlayerPrefs.SetString("GameResult", "WON");
        PlayerPrefs.Save();
    
        Debug.Log($"[BridgePusher] Loading scene index: {mainMenuSceneIndex}");
        SceneManager.LoadScene(mainMenuSceneIndex);
    
        activeGroup.Clear();
        currentPlankGroup = null;
        isAnimating = false;
    }

    void OnDrawGizmos()
    {
        if (socketCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(socketCenter.position, 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(socketCenter.position, socketCenter.right * 5f);
        }
    }
}