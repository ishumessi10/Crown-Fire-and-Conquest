using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Attach this to the Building (with RallyPoint)
public class BuildingInput : MonoBehaviour {
    public Camera cam;
    public LayerMask groundMask;
    public RallyPoint rally;

    void Reset(){ cam = Camera.main; }

    bool RMBDown(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current!=null && Mouse.current.rightButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(1);
        #endif
    }

    void Update(){
        if (!cam) cam = Camera.main;

        // If this building is selected, allow RMB to set rally
        // (Reuse your Selectable: we’ll mark buildings selectable too)
        var sel = GetComponent<Selectable>();
        if (!sel || !sel.isSelected) return;

        if (RMBDown() && Physics.Raycast(cam.ScreenPointToRay(MousePos()), out var hit, 500f, groundMask)){
            rally?.SetPoint(hit.point);
        }
    }

    Vector2 MousePos(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current!=null ? Mouse.current.position.ReadValue() : Vector2.zero;
        #else
        return (Vector2)Input.mousePosition;
        #endif
    }
    
    
    // put on the same building for quick test
    void OnGUI(){
        if (GUI.Button(new Rect(20,20,120,30), "Spawn Villager")){
            GetComponent<Spawner>()?.SpawnOne();
        }
    }

}
