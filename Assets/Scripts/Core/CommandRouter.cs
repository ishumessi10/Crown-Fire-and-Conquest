using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class CommandRouter : MonoBehaviour {
    public Camera cam;
    public LayerMask groundMask;
    public LayerMask enemyMask;          // set in Inspector
    public SelectionManager selection;
    public MoveMarker markerPrefab;

    bool attackMode; // press A to enable for ONE left-click command

    void Reset(){ cam = Camera.main; }

    // ---- input helpers ----
    bool RMBDown(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(1);
        #endif
    }
    bool LMBDown(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(0);
        #endif
    }
    bool ADown(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.A);
        #endif
    }
    bool ShiftHeld(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        #else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        #endif
    }
    bool SDown(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.S);
        #endif
    }
    bool HDown(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame;
        #else
        return Input.GetKeyDown(KeyCode.H);
        #endif
    }
    Vector2 MousePos(){
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        #else
        return (Vector2)Input.mousePosition;
        #endif
    }
    // -----------------------

    void Update(){
        if (!cam) cam = Camera.main;

        // Press A to arm attack mode for the next LMB
        if (ADown()) attackMode = true;

        // Allow Stop/Hold even with no selection? Up to you.
        if (selection == null || selection.Current == null || selection.Current.Count == 0) return;

        // Stop / Hold
        if (SDown()){
            foreach (var sel in selection.Current){
                var mover = sel.GetComponent<UnitMover>();
                if (mover) mover.StopNow();
            }
        }
        if (HDown()){
            bool anyNotHolding = false;
            foreach (var sel in selection.Current){
                var mover = sel.GetComponent<UnitMover>();
                if (mover && !mover.IsHolding){ anyNotHolding = true; break; }
            }
            foreach (var sel in selection.Current){
                var mover = sel.GetComponent<UnitMover>();
                if (mover) mover.SetHold(anyNotHolding);
            }
        }

        // --- A + LMB : Attack command (enemy or ground = attack-move) ---
        if (attackMode && LMBDown()){
            // prevent SelectionManager from clearing/dragging on this click
            selection.BlockNextClick();

            var mp = MousePos();
            var ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

            // Left-click on ENEMY while armed -> direct attack target
            if (Physics.Raycast(ray, out var hitEnemy, 500f, enemyMask)){
                foreach (var sel in selection.Current){
                    var atk = sel.GetComponent<AttackUnit>();
                    if (atk){
                        var t = hitEnemy.collider.GetComponentInParent<Targetable>();
                        if (t) atk.IssueAttackTarget(t);
                    }
                }
                attackMode = false;
                return;
            }

            // Else left-click on GROUND while armed -> attack-move to point
            if (Physics.Raycast(ray, out var hit, 500f, groundMask)){
                int i = 0; float spacing = 0.8f;
                foreach (var sel in selection.Current){
                    var atk = sel.GetComponent<AttackUnit>();
                    var mover = sel.GetComponent<UnitMover>();
                    Vector2 offset = CircleOffset(i++, spacing);
                    var target = hit.point + new Vector3(offset.x, 0f, offset.y);

                    if (atk) atk.IssueAttackMove(target);
                    else if (mover) {
                        if (ShiftHeld()) mover.QueueMove(target);
                        else             mover.IssueMove(target);
                    }
                }
                if (markerPrefab){ var m = Instantiate(markerPrefab); m.ShowAt(hit.point); }
            }

            attackMode = false; // consume A-mode after the click
            return;
        }

        
        // --- Normal RMB move/queue (override combat) ---
        if (RMBDown()){
            var mp = MousePos();
            var ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

            if (Physics.Raycast(ray, out var hit, 500f, groundMask)){
                attackMode = false; // cancel pending A-mode

                int i = 0; float spacing = 0.8f;
                foreach (var sel in selection.Current){
                    var atk = sel.GetComponent<AttackUnit>();
                    if (atk) atk.ClearTarget();              // <- IMPORTANT

                    var mover = sel.GetComponent<UnitMover>();
                    if (!mover) continue;

                    Vector2 offset = CircleOffset(i++, spacing);
                    var target = hit.point + new Vector3(offset.x, 0f, offset.y);
                    if (ShiftHeld()) mover.QueueMove(target);
                    else             mover.IssueMove(target);
                }

                if (markerPrefab){ var m = Instantiate(markerPrefab); m.ShowAt(hit.point); }
            }
        }
    }

    Vector2 CircleOffset(int idx, float spacing){
        int ring = Mathf.FloorToInt(Mathf.Sqrt(idx));
        int steps = Mathf.Max(6, (ring + 1) * 6);
        float t = (idx % steps) / (float)steps;
        float ang = t * Mathf.PI * 2f;
        float radius = (ring + 1) * spacing;
        return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
    }
}
