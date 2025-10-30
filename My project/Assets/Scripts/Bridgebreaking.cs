using UnityEngine;

public class Bridgebreaking : MonoBehaviour
{
    [SerializeField] private float maxMass;
    private float _totalMass;
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            _totalMass += other.GetComponent<Rigidbody>().mass;
        }

        if (_totalMass >= maxMass)
        {
            foreach (var part in GetComponentsInChildren<Transform>())
            {
                part.gameObject.AddComponent<Rigidbody>();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            _totalMass -= other.GetComponent<Rigidbody>().mass;
        }
    }
}
