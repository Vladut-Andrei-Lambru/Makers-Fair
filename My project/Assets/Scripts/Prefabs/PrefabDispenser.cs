using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

[DisallowMultipleComponent]
public class ToolRack : MonoBehaviour
{
    [Header("Rack Settings")]
    public GameObject prefabToSpawn;
    public Transform holdPosition;
    public bool lockRotation = true;
    public bool lockPosition = true;
    public float respawnDelay = 0.3f;

    [Header("Poof Effect")]
    public GameObject poofEffectPrefab; // cloud or burst particle effect

    [Header("Visual")]
    public bool showGizmo = true;
    public Color gizmoColor = Color.green;

    private GameObject _heldItem;
    private XRGrabInteractable _heldGrab;
    private Rigidbody _heldRb;
    private bool _isHeld;

    void Start()
    {
        if (holdPosition == null)
            holdPosition = transform;

        SpawnItem();
    }

    void SpawnItem()
    {
        if (_heldItem != null) return;

        if (poofEffectPrefab != null)
            Instantiate(poofEffectPrefab, holdPosition.position, Quaternion.identity);

        StartCoroutine(SpawnItemDelayed(0.2f)); // short delay for poof to play
    }

    IEnumerator SpawnItemDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        _heldItem = Instantiate(prefabToSpawn, holdPosition.position, holdPosition.rotation);
        _heldGrab = _heldItem.GetComponent<XRGrabInteractable>();
        _heldRb = _heldItem.GetComponent<Rigidbody>();

        if (_heldGrab != null)
            _heldGrab.selectEntered.AddListener(OnItemGrabbed);

        _isHeld = true;
        Debug.Log($"[ToolRack] Spawned {_heldItem.name}");
    }

    void FixedUpdate()
    {
        if (_isHeld && _heldItem != null && _heldGrab != null && !_heldGrab.isSelected)
        {
            if (lockPosition)
                _heldItem.transform.position = holdPosition.position;
            if (lockRotation)
                _heldItem.transform.rotation = holdPosition.rotation;

            if (_heldRb != null)
            {
                _heldRb.linearVelocity = Vector3.zero;
                _heldRb.angularVelocity = Vector3.zero;
            }
        }
    }

    void OnItemGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        _isHeld = false;

        if (_heldGrab != null)
            _heldGrab.selectEntered.RemoveListener(OnItemGrabbed);

        _heldItem = null;
        _heldGrab = null;
        _heldRb = null;

        Invoke(nameof(SpawnItem), respawnDelay);
    }

    void OnDrawGizmos()
    {
        if (!showGizmo || holdPosition == null) return;

        Gizmos.color = _isHeld ? gizmoColor : Color.red;
        Gizmos.DrawWireSphere(holdPosition.position, 0.04f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(holdPosition.position, holdPosition.forward * 0.08f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(holdPosition.position, holdPosition.right * 0.06f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(holdPosition.position, holdPosition.up * 0.06f);
    }
}
