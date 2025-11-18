using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class CommandRouter : MonoBehaviour {
    public Camera cam;
    public LayerMask groundMask;
    public LayerMask enemyMask;          // set in Inspector
    public SelectionManager selection;
    public MoveMarker markerPrefab;

    public LayerMask resourceMask;       // set to "Resource"
    public LayerMask buildingMask;       // NEW: set this to your "Building" layer in Inspector

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

    bool PointerOverUI() {
        return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    }
    // -----------------------

    void Update(){
        if (!cam) cam = Camera.main;

        // If pointer is over any UI, skip world commands this frame
        if (PointerOverUI()){
            // Debug.Log("[Input] Over UI → world input blocked");
            return;
        }

        // Press A to arm attack mode for the next LMB
        if (ADown()) attackMode = true;

        // Allow Stop/Hold even with no selection? Up to you.
        if (selection == null || selection.Current == null || selection.Current.Count == 0) return;

        // Stop / Hold
        if (SDown()){
            foreach (var sel in selection.Current){
                var mover = sel.GetComponent<UnitMover>();
                if (mover) mover.StopNow();

                // If they were building, stop that too
                var builder = sel.GetComponent<VillagerBuilder>();
                if (builder) builder.StopBuilding();
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
                    // If they were building, stop first
                    var builder = sel.GetComponent<VillagerBuilder>();
                    if (builder) builder.StopBuilding();

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
                    // If they were building, stop first
                    var builder = sel.GetComponent<VillagerBuilder>();
                    if (builder) builder.StopBuilding();

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
                if (markerPrefab)
                {
                    var m = Instantiate(markerPrefab);
                    m.ShowAt(hit.point); 
                    m.GetComponent<ScalePing>()?.Play();
                }
            }

            attackMode = false; // consume A-mode after the click
            return;
        }

        // --- Normal RMB move/queue / gather / build ---
if (RMBDown()){
    var mp = MousePos();
    var ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

    attackMode = false; // cancel pending A-mode on RMB

    Debug.Log("[CMD] RMBDown");

    // Single raycast with NO mask first, so we see *what we hit at all*
    if (!Physics.Raycast(ray, out var hitAll, 500f)){
        Debug.Log("[CMD] RMB ray hit NOTHING");
        return;
    }

    Debug.Log($"[CMD] RMB hit collider: {hitAll.collider.name} (layer {hitAll.collider.gameObject.layer})");

    // 1) Did we hit a Constructable building?
    var construct = hitAll.collider.GetComponentInParent<Constructable>();
    if (construct != null){
        Debug.Log($"[CMD] RMB on BUILDING: {construct.name}");

        foreach (var sel in selection.Current){
            var atk = sel.GetComponent<AttackUnit>();
            if (atk) atk.ClearTarget();

            var builder = sel.GetComponent<VillagerBuilder>();
            if (!builder) continue; // skip non-villagers

            builder.OrderBuild(construct);
        }
        return; // don't also move
    }

    // 2) Did we hit a resource? (use your resourceMask)
    bool hitIsResource = ((1 << hitAll.collider.gameObject.layer) & resourceMask.value) != 0;
    if (hitIsResource){
        Debug.Log($"[CMD] RMB on RESOURCE: {hitAll.collider.name}");
        foreach (var sel in selection.Current){
            var builder = sel.GetComponent<VillagerBuilder>();
            if (builder) builder.StopBuilding();

            var harv = sel.GetComponent<VillagerHarvester>();
            if (!harv) continue;

            var node = hitAll.collider.GetComponentInParent<ResourceNode>();
            if (node) {
                var atk = sel.GetComponent<AttackUnit>();
                if (atk) atk.ClearTarget();
                harv.IssueGather(node);
            }
        }
        return;
    }

    // 3) Otherwise treat it as ground move (using groundMask + formation)
    bool hitIsGround = ((1 << hitAll.collider.gameObject.layer) & groundMask.value) != 0;
    if (hitIsGround){
        Debug.Log("[CMD] RMB on GROUND");

        int i = 0; float spacing = 0.8f;
        foreach (var sel in selection.Current){
            var atk = sel.GetComponent<AttackUnit>();
            if (atk) atk.ClearTarget();

            var builder = sel.GetComponent<VillagerBuilder>();
            if (builder) builder.StopBuilding();

            var mover = sel.GetComponent<UnitMover>();
            if (!mover) continue;

            Vector2 offset = CircleOffset(i++, spacing);
            var target = hitAll.point + new Vector3(offset.x, 0f, offset.y);
            if (ShiftHeld()) mover.QueueMove(target);
            else             mover.IssueMove(target);
        }

        if (markerPrefab){
            var m = Instantiate(markerPrefab);
            m.ShowAt(hitAll.point);
            m.GetComponent<ScalePing>()?.Play();
        }
        return;
    }

    // 4) Hit something else (like a prop on another layer) → just log it
    Debug.Log("[CMD] RMB hit something that is neither building, resource, nor ground.");
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
