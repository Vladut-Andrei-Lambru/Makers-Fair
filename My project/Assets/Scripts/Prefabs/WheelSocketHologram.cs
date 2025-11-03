using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WheelSocketHologram : MonoBehaviour
{
    [System.Serializable]
    public class WheelSocket
    {
        public Transform socketTransform;
        public bool isOccupied;
        public GameObject hologramInstance;
        public Rigidbody plankRb;
    }

    public List<WheelSocket> allSockets = new();
    public GameObject hologramPrefab;
    public float activationDistance = 5f;
    public float attractionDistance = 0.5f;
    public float attractionForce = 10f;
    public float snapDistance = 0.08f;
    public string wheelTag = "Wheel";
    public string playerTag = "MainCamera";
    public int minPlanksForSockets = 2;

    private Transform _player;
    private Transform _currentWheel;
    private Rigidbody _currentWheelRb;
    private WheelSocket _closestSocket;
    private Rigidbody _currentClosestPlank;
    private Transform _lastHeldWheel;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        if (_player == null) _player = Camera.main?.transform;

        Debug.Log("[WheelHologram] Ready. Waiting for planks to be nailed together...");
    }

    public void OnPlanksJoined(PlankGroup group)
    {
        if (group == null || group.planks.Count < minPlanksForSockets)
        {
            Debug.Log($"[WheelHologram] Group has {group?.planks.Count ?? 0} planks, need at least {minPlanksForSockets}");
            return;
        }

        foreach (var plank in group.planks)
        {
            if (plank == null) continue;

            foreach (Transform child in plank.transform)
            {
                if (child.name.ToLower().Contains("socket") && child.gameObject.activeInHierarchy)
                {
                    if (allSockets.Any(s => s.socketTransform == child))
                        continue;

                    bool occupied = child.childCount > 0 && child.GetChild(0).CompareTag(wheelTag);

                    allSockets.Add(new WheelSocket
                    {
                        socketTransform = child,
                        isOccupied = occupied,
                        plankRb = plank
                    });
                }
            }
        }

        Debug.Log($"[WheelHologram] Total sockets available: {allSockets.Count(s => !s.isOccupied)} / {allSockets.Count}");
    }

    void Update()
    {
        _currentWheel = FindHeldWheel();

        if (_currentWheel != _lastHeldWheel)
        {
            _lastHeldWheel = _currentWheel;
            _currentClosestPlank = null;
            
            if (_currentWheel != null)
            {
                Debug.Log($"[WheelHologram] Wheel grabbed!");
            }
            else
            {
                Debug.Log($"[WheelHologram] Wheel released");
            }
        }

        if (_currentWheel == null)
        {
            HideAllHolograms();
            _currentClosestPlank = null;
            return;
        }

        if (allSockets.Count == 0)
        {
            return;
        }

        _currentWheelRb = _currentWheel.GetComponent<Rigidbody>();

        Rigidbody closestPlank = GetClosestPlankToPlayer();

        if (closestPlank != null && _player != null)
        {
            Vector3 plankCenter = GetPlankCenter(closestPlank);
            float distanceToPlayer = Vector3.Distance(_player.position, plankCenter);

            if (distanceToPlayer > activationDistance)
            {
                HideAllHolograms();
                return;
            }
        }

        if (closestPlank != _currentClosestPlank)
        {
            _currentClosestPlank = closestPlank;
        }

        UpdateHologramsForPlank(closestPlank);

        var plankSockets = GetSocketsForPlank(closestPlank);

        if (plankSockets.Count > 0)
        {
            _closestSocket = GetClosestSocketToWheel(plankSockets);

            if (_closestSocket != null)
            {
                float socketDistance = Vector3.Distance(_currentWheel.position, _closestSocket.socketTransform.position);

                if (socketDistance <= snapDistance)
                {
                    SnapWheel();
                }
                else if (socketDistance < attractionDistance)
                {
                    AttractWheel();
                }
            }
        }
    }

    Transform FindHeldWheel()
    {
        var wheels = GameObject.FindGameObjectsWithTag(wheelTag);

        foreach (var wheel in wheels)
        {
            var grab = wheel.GetComponent<XRGrabInteractable>();
            if (grab != null && grab.enabled && grab.isSelected)
            {
                return wheel.transform;
            }
        }
        return null;
    }

    Vector3 GetPlankCenter(Rigidbody plank)
    {
        if (plank == null) return Vector3.zero;

        var sockets = allSockets.Where(s => s.plankRb == plank && s.socketTransform != null).ToList();

        if (sockets.Count == 0) return plank.position;

        Vector3 center = Vector3.zero;
        foreach (var socket in sockets)
        {
            center += socket.socketTransform.position;
        }
        return center / sockets.Count;
    }

    Rigidbody GetClosestPlankToPlayer()
    {
        if (_player == null) return null;

        Rigidbody closestPlank = null;
        float minDistance = float.MaxValue;

        var plankGroups = allSockets.Where(s => s.plankRb != null).GroupBy(s => s.plankRb);

        foreach (var group in plankGroups)
        {
            Vector3 avgPos = Vector3.zero;
            int count = 0;

            foreach (var socket in group)
            {
                if (socket.socketTransform != null)
                {
                    avgPos += socket.socketTransform.position;
                    count++;
                }
            }

            if (count > 0)
            {
                avgPos /= count;
                float dist = Vector3.Distance(_player.position, avgPos);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestPlank = group.Key;
                }
            }
        }

        return closestPlank;
    }

    List<WheelSocket> GetSocketsForPlank(Rigidbody plank)
    {
        if (plank == null) return new List<WheelSocket>();

        return allSockets.Where(s =>
            s.plankRb == plank &&
            !s.isOccupied &&
            s.socketTransform != null &&
            s.socketTransform.gameObject.activeInHierarchy
        ).ToList();
    }

    WheelSocket GetClosestSocketToWheel(List<WheelSocket> sockets)
    {
        if (_currentWheel == null || sockets.Count == 0) return null;

        WheelSocket closest = null;
        float minDist = float.MaxValue;

        foreach (var socket in sockets)
        {
            float dist = Vector3.Distance(_currentWheel.position, socket.socketTransform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = socket;
            }
        }
        return closest;
    }

    void UpdateHologramsForPlank(Rigidbody plank)
    {
        if (plank == null)
        {
            HideAllHolograms();
            return;
        }

        var plankSockets = GetSocketsForPlank(plank);
        WheelSocket closestToWheel = plankSockets.Count > 0 ? GetClosestSocketToWheel(plankSockets) : null;

        foreach (var socket in allSockets)
        {
            bool shouldShow = (socket == closestToWheel && !socket.isOccupied && socket.socketTransform != null);

            if (shouldShow)
            {
                if (socket.hologramInstance == null && hologramPrefab != null)
                {
                    socket.hologramInstance = Instantiate(
                        hologramPrefab,
                        socket.socketTransform.position,
                        socket.socketTransform.rotation,
                        socket.socketTransform
                    );
                }
            }
            else
            {
                if (socket.hologramInstance != null)
                {
                    Destroy(socket.hologramInstance);
                    socket.hologramInstance = null;
                }
            }
        }
    }

    void HideAllHolograms()
    {
        foreach (var socket in allSockets)
        {
            if (socket.hologramInstance != null)
            {
                Destroy(socket.hologramInstance);
                socket.hologramInstance = null;
            }
        }
    }

    void AttractWheel()
    {
        if (_currentWheelRb == null || _closestSocket == null) return;

        var grab = _currentWheel.GetComponent<XRGrabInteractable>();
        if (grab != null && grab.isSelected) return;

        Vector3 dir = (_closestSocket.socketTransform.position - _currentWheel.position).normalized;
        _currentWheelRb.AddForce(dir * attractionForce, ForceMode.Acceleration);

        _currentWheelRb.linearVelocity *= 0.8f;
        _currentWheelRb.angularVelocity *= 0.8f;
    }

    void SnapWheel()
    {
        if (_currentWheel == null || _closestSocket == null) return;

        Debug.Log($"[WheelHologram] Snapping wheel to {_closestSocket.socketTransform.name}");

        var wheelRb = _currentWheel.GetComponent<Rigidbody>();
        var grab = _currentWheel.GetComponent<XRGrabInteractable>();
        var plankRb = _closestSocket.plankRb;

        if (wheelRb == null || plankRb == null)
        {
            Debug.LogError("[WheelHologram] Missing rigidbodies!");
            return;
        }

        // Force release from hand first
        if (grab != null && grab.isSelected)
        {
            grab.interactionManager?.SelectExit(grab.firstInteractorSelecting, grab);
        }

        // Disable the grab component (don't destroy - just disable)
        if (grab != null)
        {
            grab.enabled = false;
            Debug.Log("[WheelHologram] Disabled XRGrabInteractable - wheel locked in socket");
        }

        // Stop velocities first
        wheelRb.linearVelocity = Vector3.zero;
        wheelRb.angularVelocity = Vector3.zero;

        // Keep wheel DYNAMIC (not kinematic) so physics works properly
        wheelRb.isKinematic = false;
        wheelRb.useGravity = false;

        // Position wheel at socket
        _currentWheel.SetParent(_closestSocket.socketTransform);
        _currentWheel.localPosition = Vector3.zero;
        _currentWheel.localRotation = Quaternion.identity;

        // Use FixedJoint to physically connect wheel to plank
        var joint = wheelRb.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = plankRb;
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;
        joint.enablePreprocessing = false;

        // Increase solver iterations for stability
        wheelRb.solverIterations = 12;
        wheelRb.solverVelocityIterations = 12;

        // Keep colliders active so wheel still collides with ground
        foreach (var col in _currentWheel.GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        // Mark socket as occupied
        _closestSocket.isOccupied = true;

        // Cleanup hologram
        if (_closestSocket.hologramInstance != null)
        {
            Destroy(_closestSocket.hologramInstance);
            _closestSocket.hologramInstance = null;
        }

        _closestSocket = null;
        Debug.Log("[WheelHologram] Wheel physically attached with FixedJoint!");
    }
}