using UnityEngine;

public class DropoffPoint : MonoBehaviour {
    [Header("What this accepts")]
    public bool acceptsAll = true;
    public ResourceType[] accepts; // ignored if acceptsAll

    [Header("Slots")]
    public Transform entrance;          // optional; where slots are centered
    [Range(1, 16)] public int maxSlots = 4;
    public float slotRadius = 1.6f;     // distance from entrance center
    public float radialPadding = 1.0f;  // 1.0 = even ring; >1 spreads them more

    bool[] occupied;

    void OnEnable(){
        occupied = new bool[Mathf.Max(1, maxSlots)];
        if (!entrance) entrance = transform; // fallback
    }

    public bool Accepts(ResourceType t){
        if (acceptsAll) return true;
        foreach (var a in accepts) if (a == t) return true;
        return false;
    }

    /// Try to reserve a free slot, preferring one in the requester's approach direction.
    public bool TryReserveSlot(out int slotIndex, out Vector3 worldPos, Vector3 requesterPos){
        slotIndex = -1; worldPos = entrance ? entrance.position : transform.position;

        // Pick the free slot whose direction best matches approach
        int best = -1; float bestDot = -2f;

        Vector3 center = entrance ? entrance.position : transform.position;
        Vector3 toReq = requesterPos - center; toReq.y = 0f;
        Vector3 prefDir = toReq.sqrMagnitude > 0.01f ? toReq.normalized : Vector3.forward;

        for (int i = 0; i < occupied.Length; i++){
            if (occupied[i]) continue;
            float ang = (i / (float)occupied.Length) * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
            float d = Vector3.Dot(dir, prefDir);
            if (d > bestDot){ bestDot = d; best = i; }
        }

        if (best == -1) return false;

        occupied[best] = true;
        slotIndex = best;

        float radius = Mathf.Max(0.5f, slotRadius * Mathf.Max(1f, radialPadding));
        float angle = (best / (float)occupied.Length) * Mathf.PI * 2f;
        Vector3 dirBest = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        worldPos = center + dirBest * radius;

        // Snap to NavMesh
        if (UnityEngine.AI.NavMesh.SamplePosition(worldPos, out var hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            worldPos = hit.position;

        return true;
    }

    public void ReleaseSlot(int slotIndex){
        if (occupied == null) return;
        if (slotIndex >= 0 && slotIndex < occupied.Length) occupied[slotIndex] = false;
    }
}
