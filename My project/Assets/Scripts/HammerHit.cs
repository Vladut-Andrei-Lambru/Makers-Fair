using UnityEngine;

public class HammerHit : MonoBehaviour
{
    public Rigidbody hammerRb;         // assign hammer rigidbody
    public string nailHeadTag = "NailHead"; // tag the nail HEAD collider

    void Reset()
    {
        if (!hammerRb) hammerRb = GetComponentInParent<Rigidbody>();
    }

    void OnCollisionEnter(Collision c)
    {
        if (!hammerRb) return;
        if (!c.collider.CompareTag(nailHeadTag)) return;

        // find NailDrive on the nail root
        var drive = c.collider.GetComponentInParent<NailDrive>();
        if (!drive) return;

        // relative speed along contact normal
        Vector3 avgNormal = Vector3.zero;
        foreach (var cp in c.contacts) avgNormal += cp.normal;
        avgNormal.Normalize();

        float relSpeed = c.relativeVelocity.magnitude;
        Vector3 hitDir = c.relativeVelocity.normalized; // direction hammer moved vs nail

        drive.DriveByHit(relSpeed, hitDir);
    }
}