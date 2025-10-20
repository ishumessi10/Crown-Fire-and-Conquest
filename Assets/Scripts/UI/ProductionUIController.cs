using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class ProductionUIController : MonoBehaviour {
    [Header("Scene refs")]
    public SelectionManager selection;        // drag from scene

    [Header("Panel")]
    public GameObject panelRoot;              // whole production panel root (setActive true/false)
    public Text buildingTitle;                // optional label, else leave null

    [Header("Buttons (catalog)")]
    public Transform catalogRoot;             // layout group parent for buttons
    public Button catalogButtonPrefab;        // a simple button prefab with an Image + Text

    [Header("Queue display")]
    public Transform queueRoot;               // layout group parent for queue icons
    public GameObject queueIconPrefab;        // the QueueIcon prefab
    public Image currentProgressFill;         // OPTIONAL: shows current's progress (bind to the first icon's Progress)

    ProductionBuilding active;
    readonly List<Button> catalogButtons = new List<Button>();
    readonly List<GameObject> queueIcons = new List<GameObject>();

    void Update(){
        var pb = GetSelectedProductionBuilding();
        if (panelRoot) panelRoot.SetActive(pb != null);

        if (pb != active){
            active = pb;
            RebuildCatalog();
            RebuildQueueIcons(force:true);
            if (buildingTitle) buildingTitle.text = active ? active.name : "";
        }

        if (active){
            RebuildQueueIcons(force:false);
            UpdateProgress();
            // Hotkeys 1..N to enqueue quickly
            HandleHotkeys();
        }
    }

    void HandleHotkeys(){
        if (active == null) return;
        for (int i=0; i<active.options.Count && i<9; i++){
            bool key = false;
            #if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = Keyboard.current;
            if (kb != null){
                if (i==0 && kb.digit1Key.wasPressedThisFrame) key = true;
                if (i==1 && kb.digit2Key.wasPressedThisFrame) key = true;
                if (i==2 && kb.digit3Key.wasPressedThisFrame) key = true;
                if (i==3 && kb.digit4Key.wasPressedThisFrame) key = true;
                if (i==4 && kb.digit5Key.wasPressedThisFrame) key = true;
                if (i==5 && kb.digit6Key.wasPressedThisFrame) key = true;
                if (i==6 && kb.digit7Key.wasPressedThisFrame) key = true;
                if (i==7 && kb.digit8Key.wasPressedThisFrame) key = true;
                if (i==8 && kb.digit9Key.wasPressedThisFrame) key = true;
            }
            #else
            if (i==0 && Input.GetKeyDown(KeyCode.Alpha1)) key = true;
            if (i==1 && Input.GetKeyDown(KeyCode.Alpha2)) key = true;
            if (i==2 && Input.GetKeyDown(KeyCode.Alpha3)) key = true;
            if (i==3 && Input.GetKeyDown(KeyCode.Alpha4)) key = true;
            if (i==4 && Input.GetKeyDown(KeyCode.Alpha5)) key = true;
            if (i==5 && Input.GetKeyDown(KeyCode.Alpha6)) key = true;
            if (i==6 && Input.GetKeyDown(KeyCode.Alpha7)) key = true;
            if (i==7 && Input.GetKeyDown(KeyCode.Alpha8)) key = true;
            if (i==8 && Input.GetKeyDown(KeyCode.Alpha9)) key = true;
            #endif
            if (key) TryEnqueue(i);
        }
    }

    ProductionBuilding GetSelectedProductionBuilding(){
        if (selection == null || selection.Current == null) return null;
        foreach (var sel in selection.Current){
            var pb = sel.GetComponent<ProductionBuilding>();
            if (pb) return pb; // first selected production building
        }
        return null;
    }

    void RebuildCatalog(){
        // clear old
        foreach (var b in catalogButtons) if (b) Destroy(b.gameObject);
        catalogButtons.Clear();

        if (active == null || catalogButtonPrefab == null || catalogRoot == null) return;

        for (int i=0; i<active.options.Count; i++){
            int idx = i;
            var btn = Instantiate(catalogButtonPrefab, catalogRoot);
            catalogButtons.Add(btn);

            var txt = btn.GetComponentInChildren<Text>();
            if (txt) txt.text = active.options[i].displayName;

            var img = btn.GetComponentInChildren<Image>();
            if (img && active.options[i].icon) img.sprite = active.options[i].icon;

            btn.onClick.AddListener(()=> TryEnqueue(idx));
        }
    }

    void TryEnqueue(int idx){
        if (active) active.Enqueue(idx);
    }

    void RebuildQueueIcons(bool force){
        if (active == null || queueRoot == null) return;

        var icons = active.GetQueueIcons();
        if (!force && icons.Count == queueIcons.Count) return;

        // rebuild list
        foreach (var go in queueIcons) if (go) Destroy(go);
        queueIcons.Clear();

        for (int i=0; i<icons.Count; i++){
            var go = Instantiate(queueIconPrefab, queueRoot);
            queueIcons.Add(go);
            var imgs = go.GetComponentsInChildren<Image>(true);
            foreach (var im in imgs){
                if (im.gameObject.name.ToLower().Contains("progress")){
                    // progress fill will be set in UpdateProgress
                    if (i == 0 && currentProgressFill != null) currentProgressFill = im;
                } else {
                    if (icons[i]) im.sprite = icons[i];
                }
            }
        }
    }

    void UpdateProgress(){
        if (currentProgressFill == null) return;
        currentProgressFill.fillAmount = active.CurrentProgress01;
    }
}
