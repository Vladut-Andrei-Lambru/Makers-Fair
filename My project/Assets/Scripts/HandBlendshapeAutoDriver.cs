using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandBlendshapeAutoDriver : MonoBehaviour
{
    public InputActionReference gripValue;   // XRI Left/Right -> Select Value (float)
    public InputActionReference pinchValue;  // XRI Left/Right -> Activate Value (float)

    // tweak these if your blendshape names differ
    public string thumbMask="thumb", indexMask="index", middleMask="middle", ringMask="ring", littleMask="little", curlMask="curl";

    SkinnedMeshRenderer skinned;
    int thumb=-1,index=-1,middle=-1,ring=-1,little=-1;

    void Awake(){
        skinned = GetComponentInChildren<SkinnedMeshRenderer>();
        if (!skinned) { Debug.LogWarning("No SkinnedMeshRenderer found on hand."); return; }

        string[] names = Enumerable.Range(0, skinned.sharedMesh.blendShapeCount)
            .Select(i => skinned.sharedMesh.GetBlendShapeName(i).ToLower()).ToArray();

        thumb  = Find(names, thumbMask,  curlMask);
        index  = Find(names, indexMask,  curlMask);
        middle = Find(names, middleMask, curlMask);
        ring   = Find(names, ringMask,   curlMask);
        little = Find(names, littleMask, curlMask);
    }
    static int Find(string[] names, string a, string b){
        for (int i=0;i<names.Length;i++) if (names[i].Contains(a) && names[i].Contains(b)) return i;
        for (int i=0;i<names.Length;i++) if (names[i].Contains(a)) return i;
        return -1;
    }

    void OnEnable(){ gripValue?.action?.Enable(); pinchValue?.action?.Enable(); }
    void OnDisable(){ gripValue?.action?.Disable(); pinchValue?.action?.Disable(); }

    void LateUpdate(){
        if (!skinned) return;
        float grip  = Mathf.Clamp01(gripValue?.action?.ReadValue<float>()  ?? 0f) * 100f;
        float pinch = Mathf.Clamp01(pinchValue?.action?.ReadValue<float>() ?? 0f) * 100f;

        // Grip curls middle/ring/little
        Set(middle, grip); Set(ring, grip); Set(little, grip);
        // Pinch drives index & thumb
        float pinchish = Mathf.Max(grip * 0.6f, pinch);
        Set(index, pinchish);
        Set(thumb, Mathf.Max(grip * 0.4f, pinch));
    }
    void Set(int idx, float v){ if (idx >= 0) skinned.SetBlendShapeWeight(idx, v); }
}
