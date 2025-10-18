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

    bool RMBDown() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(1);
        #endif
    }

    Vector2 MousePos() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        #else
        return (Vector2)Input.mousePosition;
        #endif
    }

    void Update() {
        if (!cam) cam = Camera.main;
        if (selection == null || selection.Current == null || selection.Current.Count == 0) return;

        if (RMBDown()) {
            var mp = MousePos();
            var ray = cam.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

            if (Physics.Raycast(ray, out var hit, 500f, groundMask)) {
                int i = 0;
                float spacing = 0.8f;

                foreach (var sel in selection.Current) {
                    var mover = sel.GetComponent<UnitMover>();
                    if (mover) {
                        Vector2 offset = CircleOffset(i++, spacing);
                        mover.IssueMove(hit.point + new Vector3(offset.x, 0, offset.y));
                    }
                }

                if (markerPrefab) {
                    var m = Instantiate(markerPrefab);
                    m.ShowAt(hit.point);
                }
            }
        }
    }

    Vector2 CircleOffset(int idx, float spacing) {
        int ring = Mathf.FloorToInt(Mathf.Sqrt(idx));
        int steps = Mathf.Max(6, (ring + 1) * 6);
        float t = (idx % steps) / (float)steps;
        float ang = t * Mathf.PI * 2f;
        float radius = (ring + 1) * spacing;
        return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
    }
}
