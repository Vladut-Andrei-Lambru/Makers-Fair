using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketListener : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    public Rigidbody hostPlank;       // the plank’s rigidbody
    public Transform holeTransform;   // this transform (the hole/orientation)

    void Reset()
    {
        if (!socket) socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        if (!holeTransform) holeTransform = transform;
        if (!hostPlank) hostPlank = GetComponentInParent<Rigidbody>();
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnSelectEntered);
        socket.selectExited.AddListener(OnSelectExited);
    }
    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnSelectEntered);
        socket.selectExited.RemoveListener(OnSelectExited);
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        var nail = args.interactableObject.transform.GetComponent<NailDrive>();
        if (nail) nail.BindSocket(socket, holeTransform, hostPlank);
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        var nail = args.interactableObject.transform.GetComponent<NailDrive>();
        if (nail) nail.UnbindSocket();
    }
}