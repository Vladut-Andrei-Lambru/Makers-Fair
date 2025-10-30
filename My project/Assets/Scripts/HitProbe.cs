using UnityEngine;

public class HitProbe : MonoBehaviour
{
    void OnCollisionEnter(Collision c)  { Debug.Log($"HitProbe COLLISION with {c.collider.name}"); }
    void OnTriggerEnter(Collider other) { Debug.Log($"HitProbe TRIGGER with {other.name}"); }
}