using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class RTSCameraController : MonoBehaviour {
    [Header("Move & Zoom")]
    public float panSpeed = 15f;
    public float edgePanSpeed = 20f;
    public int edgeSize = 12;             // px from screen edge
    public bool edgePan = true;
    public float zoomSpeed = 400f;
    public float minHeight = 8f, maxHeight = 60f;

    [Header("Angles")]
    public float tiltDegrees = 55f;       // camera pitch
    public bool lockToYPlane = true;      // keep constant pitch

    [Header("Bounds (optional)")]
    public Vector2 xzMin = new Vector2(-200, -200);
    public Vector2 xzMax = new Vector2( 200,  200);

    Camera cam;
    void Awake(){
        cam = GetComponent<Camera>();
        transform.rotation = Quaternion.Euler(tiltDegrees, transform.eulerAngles.y, 0f);
    }

    // Arrow-key axes
    float AxisHoriz(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var k = Keyboard.current;
        if (k == null) return 0f;
        return (k.rightArrowKey.isPressed?1:0) + (k.leftArrowKey.isPressed?-1:0);
        #else
        return (Input.GetKey(KeyCode.RightArrow)?1:0) + (Input.GetKey(KeyCode.LeftArrow)?-1:0);
        #endif
    }
    float AxisVert(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        var k = Keyboard.current;
        if (k == null) return 0f;
        return (k.upArrowKey.isPressed?1:0) + (k.downArrowKey.isPressed?-1:0);
        #else
        return (Input.GetKey(KeyCode.UpArrow)?1:0) + (Input.GetKey(KeyCode.DownArrow)?-1:0);
        #endif
    }

    float Scroll(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current!=null ? Mouse.current.scroll.ReadValue().y : 0f;
        #else
        return Input.mouseScrollDelta.y;
        #endif
    }
    Vector2 MousePos(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current!=null ? Mouse.current.position.ReadValue() : Vector2.zero;
        #else
        return (Vector2)Input.mousePosition;
        #endif
    }

    void Update(){
        // keyboard pan (arrows)
        Vector3 move = new Vector3(AxisHoriz(), 0f, AxisVert());

        // edge pan
        if (edgePan){
            var mp = MousePos();
            if (mp.x <= edgeSize) move += Vector3.left;
            else if (mp.x >= Screen.width - edgeSize) move += Vector3.right;
            if (mp.y <= edgeSize) move += Vector3.back;
            else if (mp.y >= Screen.height - edgeSize) move += Vector3.forward;
        }

        // move along ground plane
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;
        Vector3 fwd   = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 delta = (right * move.x + fwd * move.z) * panSpeed * Time.deltaTime;
        transform.position += delta;

        // zoom
        float s = Scroll();
        if (Mathf.Abs(s) > 0.001f){
            float newY = Mathf.Clamp(transform.position.y - s * zoomSpeed * Time.deltaTime, minHeight, maxHeight);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // clamp to bounds
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, xzMin.x, xzMax.x),
            transform.position.y,
            Mathf.Clamp(transform.position.z, xzMin.y, xzMax.y)
        );

        if (lockToYPlane){
            transform.rotation = Quaternion.Euler(tiltDegrees, transform.eulerAngles.y, 0f);
        }
    }
}
