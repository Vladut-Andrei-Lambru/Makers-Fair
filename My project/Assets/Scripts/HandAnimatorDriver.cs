using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimatorDriver : MonoBehaviour
{
    public InputActionReference gripValue;   // XRI Left/RightHand -> Interaction/Select Value
    public InputActionReference pinchValue;  // XRI Left/RightHand -> Interaction/Activate Value
    public string gripParam = "Grip";        // Change if your Animator uses different names
    public string pinchParam = "Pinch";

    Animator anim; int gripHash, pinchHash; float gSm, pSm;

    void Awake() {
        anim = GetComponent<Animator>();
        gripHash = Animator.StringToHash(gripParam);
        pinchHash = Animator.StringToHash(pinchParam);
    }
    void OnEnable(){ gripValue?.action?.Enable(); pinchValue?.action?.Enable(); }
    void OnDisable(){ gripValue?.action?.Disable(); pinchValue?.action?.Disable(); }

    void Update() {
        if (!anim) return;
        float g = gripValue?.action?.ReadValue<float>() ?? 0f;
        float p = pinchValue?.action?.ReadValue<float>() ?? 0f;
        // small smoothing so it feels natural
        gSm = Mathf.Lerp(gSm, g, 12f * Time.deltaTime);
        pSm = Mathf.Lerp(pSm, p, 12f * Time.deltaTime);
        anim.SetFloat(gripHash, gSm);
        anim.SetFloat(pinchHash, pSm);
    }
}
