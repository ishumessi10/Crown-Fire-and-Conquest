using UnityEngine;

public class PlaceableBuilding : MonoBehaviour {
    [Header("Cost & Footprint")]
    public ResourceCost[] cost;
    public Vector3 footprint = new Vector3(3, 1, 3);     // fallback if no collider
    public BoxCollider footprintCollider;                // assign the child BoxCollider
    public LayerMask blockLayers;                        // e.g., Building, Obstacles
    public bool requiresNavMesh = true;

    [Header("Debug")]
    public bool debugPlacement = false;

    // Computes the world-space center/halfExtents the overlap will use,
    // based on the footprint collider if provided; otherwise the vector.
    void GetFootprintWorld(Vector3 atPos, out Vector3 center, out Vector3 half){
        if (footprintCollider){
            // use collider size (in local space) and transform it
            var col = footprintCollider;
            var lossy = col.transform.lossyScale;
            half = Vector3.Scale(col.size, lossy) * 0.5f;
            // collider.center is in local space of the collider
            center = col.transform.TransformPoint(col.center);
            // move the computed center horizontally to the preview 'atPos'
            // keep the collider's Y (so height stays correct)
            center = new Vector3(atPos.x, center.y, atPos.z);
        } else {
            half = footprint * 0.5f;
            center = atPos + Vector3.up * half.y; // lift so it’s above ground
        }
    }

    public bool CanPlaceHere(Vector3 atPos){
        GetFootprintWorld(atPos, out var center, out var half);

        // Check overlaps against blocking layers, ignore triggers
        const int CAP = 32;
        var hits = new Collider[CAP];
        int n = Physics.OverlapBoxNonAlloc(
            center, half, hits, Quaternion.identity,
            blockLayers, QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < n; i++){
            var h = hits[i];
            if (!h) continue;
            // Ignore anything that belongs to THIS preview/final object
            if (h.transform.root == transform.root) continue;

            if (debugPlacement){
                Debug.Log($"[Placeable] Blocked by: {h.name} (layer {LayerMask.LayerToName(h.gameObject.layer)})", h);
            }
            return false;
        }

        if (requiresNavMesh &&
            !UnityEngine.AI.NavMesh.SamplePosition(atPos, out var _, 0.6f, UnityEngine.AI.NavMesh.AllAreas))
            return false;

        return true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected(){
        if (!debugPlacement) return;
        var pos = Application.isPlaying ? transform.position : transform.position;
        GetFootprintWorld(pos, out var center, out var half);
        Gizmos.color = new Color(1, 0.5f, 0, 0.25f);
        Gizmos.DrawCube(center, half*2f);
        Gizmos.color = new Color(1, 0.5f, 0, 1f);
        Gizmos.DrawWireCube(center, half*2f);
    }
#endif
}
