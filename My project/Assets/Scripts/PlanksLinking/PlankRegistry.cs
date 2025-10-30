using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
public class PlankRegistry : MonoBehaviour
{
    public XRGrabInteractable grab;

    void Awake()
    {
        if (!grab)
            grab = GetComponent<XRGrabInteractable>();
    }
}