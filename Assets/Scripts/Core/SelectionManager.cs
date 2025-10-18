using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class SelectionManager : MonoBehaviour {
    public Camera cam;                  // Assign Main Camera
    public LayerMask selectableMask;
    public Image selectionBox;          // Drag the UI Image here

    private readonly HashSet<Selectable> selected = new HashSet<Selectable>();
    private RectTransform boxRT;
    private Canvas canvas;
    private RectTransform canvasRT;
    private Vector2 dragStart;
    private bool dragging;

    void Reset() { cam = Camera.main; }

    void Start() {
        if (selectionBox) {
            canvas = selectionBox.canvas;
            canvasRT = canvas.GetComponent<RectTransform>();
            boxRT = selectionBox.rectTransform;
            selectionBox.gameObject.SetActive(false);
        }
    }

    bool LMBDown() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        #else
        return Input.GetMouseButtonDown(0);
        #endif
    }
    bool LMB() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.isPressed;
        #else
        return Input.GetMouseButton(0);
        #endif
    }
    bool LMBUp() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        #else
        return Input.GetMouseButtonUp(0);
        #endif
    }
    bool Shift() {
        #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        #else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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

        if (LMBDown()) {
            dragStart = MousePos();
            ToggleBox(true);
            dragging = true;
        }

        if (dragging && LMB()) {
            UpdateBox(dragStart, MousePos());
        }

        if (dragging && LMBUp()) {
            dragging = false;
            ToggleBox(false);

            var end = MousePos();
            if (Vector2.Distance(dragStart, end) < 6f) {
                // Click select
                var ray = cam.ScreenPointToRay(new Vector3(end.x, end.y, 0f));
                if (Physics.Raycast(ray, out var hit, 500f, selectableMask)) {
                    var s = hit.collider.GetComponentInParent<Selectable>();
                    if (!Shift()) Clear();
                    if (s) Add(s);
                } else if (!Shift()) {
                    Clear();
                }
            } else {
                // Marquee select
                if (!Shift()) Clear();
                var r = BuildCanvasRect(dragStart, end);
                foreach (var s in FindObjectsOfType<Selectable>()) {
                    var sp = cam.WorldToScreenPoint(s.transform.position);
                    // Convert that screen point into the same canvas space the rect uses
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, sp, CanvasCamera(), out var local);
                    if (r.Contains(local, true)) Add(s);
                }
            }
        }
    }

    void Add(Selectable s) { if (selected.Add(s)) s.SetSelected(true); }
    public void Clear() { foreach (var s in selected) s.SetSelected(false); selected.Clear(); }
    public IReadOnlyCollection<Selectable> Current => selected;

    void ToggleBox(bool on) {
        if (selectionBox) selectionBox.gameObject.SetActive(on);
    }

    // Convert two screen positions to a Rect in the canvas's local space
    Rect BuildCanvasRect(Vector2 a, Vector2 b) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, a, CanvasCamera(), out var p1);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, b, CanvasCamera(), out var p2);
        var min = Vector2.Min(p1, p2);
        var max = Vector2.Max(p1, p2);
        return new Rect(min, max - min); // position = min, size = max-min (canvas local space)
    }

    void UpdateBox(Vector2 a, Vector2 b) {
        if (!boxRT) return;
        var r = BuildCanvasRect(a, b);
        // With pivot (0.5,0.5), set anchoredPosition to rect center and sizeDelta to size
        boxRT.anchoredPosition = r.center;
        boxRT.sizeDelta = r.size;
    }

    // The camera that should be used for ScreenPointToLocalPointInRectangle
    Camera CanvasCamera() {
        if (canvas == null) return null;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera) return canvas.worldCamera ? canvas.worldCamera : cam;
        // World Space
        return cam;
    }
}
