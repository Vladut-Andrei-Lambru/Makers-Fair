using UnityEngine;

public class KeepMilkStraight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0,0,0));
    }
}
