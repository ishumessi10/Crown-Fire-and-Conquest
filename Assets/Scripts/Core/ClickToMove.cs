using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class ClickToMove : MonoBehaviour {
    public Camera cam;
    public LayerMask groundMask;
    public SelectionController selection;

    // 👇 Add this line
    public MoveMarker markerPrefab;

    void Reset(){ cam = Camera.main; }

    bool RMBDown(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(1);
        #endif
    }

    void Update(){
        if (!cam) cam = Camera.main;
        if (selection == null || selection.Current == null) return;

        if (RMBDown()){
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 500f, groundMask)){
                var mover = selection.Current.GetComponent<UnitMover>();
                if (mover) mover.IssueMove(hit.point);

                // 👇 Spawn the visual ping
                if (markerPrefab){
                    var m = Instantiate(markerPrefab);
                    m.ShowAt(hit.point);
                }
            }
        }
    }
}
