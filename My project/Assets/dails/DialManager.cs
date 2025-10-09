using Unity.VRTemplate;
using UnityEngine;

public class DialManager : MonoBehaviour
{
    [SerializeField] private float CameraSpeed, ZoomSpeed;
    [SerializeField] private GameObject DialHorizontal, DialVertical, DialZoom;
    [SerializeField] private Camera ImageCamera;
    
    private XRKnobCustom DialHoriKnob, DialVertKnob, DialZoomKnob;
    private float xValue, yValue, zValue;
    private Camera camera;
    void Start()
    {
        DialHoriKnob = DialHorizontal.GetComponentInChildren<XRKnobCustom>();
        DialVertKnob = DialVertical.GetComponentInChildren<XRKnobCustom>();
        DialZoomKnob = DialZoom.GetComponentInChildren<XRKnobCustom>();
        
    }

    // Update is called once per frame
    void Update()
    {
        xValue = (DialHoriKnob.value - 0.5f) * CameraSpeed * ImageCamera.orthographicSize * Time.deltaTime;
        zValue = (DialVertKnob.value - 0.5f) * CameraSpeed * ImageCamera.orthographicSize * Time.deltaTime;
        yValue = (DialZoomKnob.value - 0.5f) * ZoomSpeed * Time.deltaTime;
        
        ImageCamera.transform.Translate(xValue, zValue, 0);
        ImageCamera.orthographicSize += yValue;
    }
}
