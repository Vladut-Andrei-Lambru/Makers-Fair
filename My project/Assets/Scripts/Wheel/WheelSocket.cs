using UnityEngine;

public class WheelSocket : MonoBehaviour
{
    [Header("Socket Settings")]
    public float snapRadius = 0.05f;
    public bool visualizeSockets = true;
    
    private bool _isOccupied = false;
    private NailBehavior _attachedNail;

    public bool IsOccupied => _isOccupied;

    public void AttachNail(NailBehavior nail)
    {
        if (_isOccupied) return;
        
        _attachedNail = nail;
        _isOccupied = true;
    }

    public void MarkPermanent()
    {
        _isOccupied = true;
    }

    void OnDrawGizmos()
    {
        if (!visualizeSockets) return;
        
        Gizmos.color = _isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, snapRadius);
        
        // Draw direction arrow
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 0.1f);
    }
}