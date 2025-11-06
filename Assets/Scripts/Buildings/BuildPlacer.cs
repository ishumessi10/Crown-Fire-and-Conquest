using UnityEngine;

public class BuildPlacer : MonoBehaviour {
    public Camera cam;
    public LayerMask groundMask;

    GameObject ghost;
    PlaceableBuilding ghostPB;
    GameObject placePrefab;                 // <-- original prefab to place
    bool active;

    void Reset(){ cam = Camera.main; }

    public void BeginPlacement(GameObject buildingPrefab){
        if (!buildingPrefab){
            Debug.LogError("[BuildPlacer] BeginPlacement called with null prefab");
            return;
        }
        // clean previous
        if (ghost) Destroy(ghost);

        placePrefab = buildingPrefab;       // <-- remember the ORIGINAL
        ghost = Instantiate(buildingPrefab);
        ghostPB = ghost.GetComponent<PlaceableBuilding>();
        if (!ghostPB){
            Debug.LogError("[BuildPlacer] Prefab has no PlaceableBuilding on ROOT: " + buildingPrefab.name);
        }

        // mark as ghost so 'House' etc. don't grant pop on spawn
        if (!ghost.GetComponent<PlacementGhost>()) ghost.AddComponent<PlacementGhost>();
        Ghostify(ghost);
        SetGhostState(false);
        active = true;
    }

    void SetGhostState(bool ok)
    {
        if (!ghost) return;
        var color = ok ? new Color(0f, 1f, 0f, 0.45f) : new Color(1f, 0f, 0f, 0.45f);
        foreach (var r in ghost.GetComponentsInChildren<Renderer>(true))
        {
            if (!r || !r.material) continue;
            // URP Lit uses _BaseColor; Standard uses _Color — try both
            if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", color);
            if (r.material.HasProperty("_Color")) r.material.SetColor("_Color", color);
            // make sure it’s transparent
            // (skip heavy pipeline changes; good enough for prototyping)
        }
    }
    
    void SetLayerRecursively(GameObject go, int layer){
    foreach (var t in go.GetComponentsInChildren<Transform>(true))
        t.gameObject.layer = layer;
}

void Ghostify(GameObject go){
    // Put the whole ghost on the Ghost layer
    int ghostLayer = LayerMask.NameToLayer("Ghost");
    if (ghostLayer >= 0) SetLayerRecursively(go, ghostLayer);

    // No carving, no physics collisions
    foreach (var obs in go.GetComponentsInChildren<UnityEngine.AI.NavMeshObstacle>(true))
        obs.enabled = false;

    foreach (var col in go.GetComponentsInChildren<Collider>(true)) {
        // easiest: disable colliders during preview
        col.enabled = false;
        // (alternative: col.isTrigger = true;)
    }

    // Make sure there’s no Rigidbody doing physics
    foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
        rb.isKinematic = true;
}


    void Update(){
        if (!active || !ghost) return;
        if (!cam) cam = Camera.main;

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 500f, groundMask)){
            var pos = hit.point;
            // simple grid snap
            pos.x = Mathf.Round(pos.x * 2f) / 2f;
            pos.z = Mathf.Round(pos.z * 2f) / 2f;
            ghost.transform.position = pos;

            bool ok = ghostPB ? ghostPB.CanPlaceHere(pos) : true;
            SetGhostState(ok);

            // LMB place
            if (ok && Input.GetMouseButtonDown(0)){
                // Pay cost safely
                var pbOnSource = placePrefab.GetComponent<PlaceableBuilding>();
                if (!pbOnSource){
                    Debug.LogError("[BuildPlacer] Source prefab missing PlaceableBuilding: " + placePrefab.name);
                    return;
                }

                bool paid = true;
                if (ResourceBank.I){
                    paid = ResourceBank.I.TrySpend(pbOnSource.cost);
                } else {
                    Debug.LogWarning("[BuildPlacer] No ResourceBank in scene; placing for free.");
                }

                if (!paid){
                    Debug.Log("[BuildPlacer] Not enough resources.");
                    return;
                }

                // Place the ORIGINAL prefab (not the ghost)
                var final = Instantiate(placePrefab, pos, Quaternion.identity);
                final.name = placePrefab.name; // clean name

                // done
                Destroy(ghost);
                ghost = null; ghostPB = null; placePrefab = null;
                active = false;
                return;
            }
        }

        // RMB cancel
        if (Input.GetMouseButtonDown(1)){
            Destroy(ghost);
            ghost = null; ghostPB = null; placePrefab = null;
            active = false;
        }
    }
}
