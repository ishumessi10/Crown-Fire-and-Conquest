using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class SelectionController : MonoBehaviour {
    public Camera cam;
    public LayerMask selectableMask;
    private Selectable current;

    void Reset(){ cam = Camera.main; }
    bool LMBDown() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(0);
        #endif
    }

    void Update(){
        if (!cam) cam = Camera.main;
        if (LMBDown()){
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 500f, selectableMask)){
                if (current) current.SetSelected(false);
                current = hit.collider.GetComponentInParent<Selectable>();
                if (current) current.SetSelected(true);
                Debug.Log("[Select] " + hit.collider.name);
            }
        }
    }
    public Selectable Current => current;
}
