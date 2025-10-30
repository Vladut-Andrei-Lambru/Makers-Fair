using UnityEngine;

public class Uprightmilk1 : MonoBehaviour
{
    void Update()
    { 
        Mathf.Abs(transform.up.x);
        Mathf.Abs(transform.up.z);

        if (Mathf.Abs(transform.up.x) > 0.2f || Mathf.Abs(transform.up.z) > 0.2f)
        {
            Debug.Log("fell over");
        }
    }
}
