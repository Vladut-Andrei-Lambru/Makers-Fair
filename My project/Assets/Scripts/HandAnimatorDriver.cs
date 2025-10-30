using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimatorDriver : MonoBehaviour
{
    public InputActionReference gripValue;   // XRI Left/RightHand -> Interaction/Select Value
    public InputActionReference pinchValue;  // XRI Left/RightHand -> Interaction/Activate Value
    public string gripParam = "Grip";        // Change if your Animator uses different names
    public string pinchParam = "Pinch";

    Animator _anim; int _gripHash, _pinchHash; float _gSm, _pSm;

    void Awake() {
        _anim = GetComponent<Animator>();
        _gripHash = Animator.StringToHash(gripParam);
        _pinchHash = Animator.StringToHash(pinchParam);
    }
    void OnEnable(){ gripValue?.action?.Enable(); pinchValue?.action?.Enable(); }
    void OnDisable(){ gripValue?.action?.Disable(); pinchValue?.action?.Disable(); }

    void Update() {
        if (!_anim) return;
        float g = gripValue?.action?.ReadValue<float>() ?? 0f;
        float p = pinchValue?.action?.ReadValue<float>() ?? 0f;
        // small smoothing so it feels natural
        _gSm = Mathf.Lerp(_gSm, g, 12f * Time.deltaTime);
        _pSm = Mathf.Lerp(_pSm, p, 12f * Time.deltaTime);
        _anim.SetFloat(_gripHash, _gSm);
        _anim.SetFloat(_pinchHash, _pSm);
    }
}
