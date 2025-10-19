using UnityEngine;

public class JoinPoint : MonoBehaviour
{
    public Rigidbody hostPlank; // rigidbody of Plank B

    void Reset()
    {
        if (!hostPlank) hostPlank = GetComponentInParent<Rigidbody>();
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        var nail = other.GetComponentInParent<NailDrive>();
        if (nail) nail.RegisterJoinCandidate(hostPlank);
    }

    void OnTriggerExit(Collider other)
    {
        var nail = other.GetComponentInParent<NailDrive>();
        if (nail) nail.ClearJoinCandidate(hostPlank);
    }
}