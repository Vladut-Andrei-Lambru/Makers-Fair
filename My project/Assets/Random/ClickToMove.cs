using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ClickToMove : MonoBehaviour 
{
    private NavMeshAgent _agent;
    private Camera _cam;

    void Awake() {
        _agent = GetComponent<NavMeshAgent>();
        _cam = Camera.main; // finds the camera tagged MainCamera
    }

    void Update() 
    {
        if (Input.GetMouseButtonDown(0) && _cam is not null) 
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) 
            {
                _agent.SetDestination(hit.point);
            }
        }
    }
}