using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class CommandRouter : MonoBehaviour {
    public Camera cam;
    public LayerMask groundMask;
    public SelectionManager selection;
    public MoveMarker markerPrefab;

    void Reset() { cam = Camera.main; }

    // ---- Input helpers (New Input System or Legacy) ----
    bool RMBDown() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(1);
        #endif
    }
    bool ShiftHeld() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        #else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        #endif
    }
    bool SDown() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.S);
        #endif
    }
    bool HDown() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.H);
        #endif
    }
    Vector2 MousePos() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        #else
        return (Vector2)Input.mousePosition;
        #endif
    }
    // ----------------------------------------------------

    void Update() {
        if (!cam) cam = Camera.main;
        if (selection == null || selection.Current == null || selection.Current.Count == 0) {
            // still allow camera etc.
            return;
        }

        // --- Stop (S) ---
        if (SDown()) {
            foreach (var sel in selection.Current) {
                var mover = sel.GetComponent<UnitMover>();
                if (mover) mover.StopNow();
            }
        }

        // --- Hold toggle (H) ---
        if (HDown()) {
            // If any selected is NOT holding, turn hold ON for all; otherwise turn OFF for all
            bool anyNotHolding = false;
            foreach (var sel in selection.Current) {
                var mover = sel.GetComponent<UnitMover>();
                if (mover && !mover.IsHolding) { anyNotHolding = true; break; }
            }
            bool newState = anyNotHolding; // ON if any not holding, else OFF
            foreach (var sel in selection.Current) {
                var mover = sel.GetComponent<UnitMover>();
                if (mover) mover.SetHold(newState);
            }
        }

        // --- RMB issue move / queue ---
        if (RMBDown()) {
            var mp = MousePos();
            var ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

            if (Physics.Raycast(ray, out var hit, 500f, groundMask)) {
                int i = 0; float spacing = 0.8f;

                foreach (var sel in selection.Current) {
                    var mover = sel.GetComponent<UnitMover>();
                    if (!mover) continue;

                    Vector2 offset = CircleOffset(i++, spacing);
                    var target = hit.point + new Vector3(offset.x, 0f, offset.y);

                    if (ShiftHeld()) mover.QueueMove(target);   // add waypoint
                    else             mover.IssueMove(target);    // replace path (ignored if holding)
                }

                if (markerPrefab) { var m = Instantiate(markerPrefab); m.ShowAt(hit.point); }
            }
        }
    }

    // simple spacing so units don't stack
    Vector2 CircleOffset(int idx, float spacing) {
        int ring = Mathf.FloorToInt(Mathf.Sqrt(idx));
        int steps = Mathf.Max(6, (ring + 1) * 6);
        float t = (idx % steps) / (float)steps;
        float ang = t * Mathf.PI * 2f;
        float radius = (ring + 1) * spacing;
        return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
    }
}
