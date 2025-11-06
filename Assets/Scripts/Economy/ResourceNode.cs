using UnityEngine;

public class ResourceNode : MonoBehaviour {
    public ResourceType type;
    public int amount = 1000;
    public float secondsPerTick = 1.2f;
    public int yieldPerTick = 5;

    [Header("Workers")]
    [Range(1, 12)] public int maxWorkers = 3;
    [Tooltip("Radius from center where workers stand to harvest")]
    public float gatherRadius = 1.2f;
    [Tooltip("Extra spacing multiplier between workers")]
    public float radialPadding = 1.0f;

    bool[] occupied; // slot occupancy

    void OnEnable(){
        if (maxWorkers < 1) maxWorkers = 1;
        occupied = new bool[maxWorkers];
    }

    // Try to reserve a slot; returns true + slotIndex + worldPos
    public bool TryReserveWorker(out int slotIndex, out Vector3 worldPos, Vector3 requesterPos){
        slotIndex = -1; worldPos = transform.position;

        // Find a free slot; prefer the one whose direction is closest to requester
        int best = -1; float bestDot = -2f;

        Vector3 toReq = (requesterPos - transform.position);
        toReq.y = 0f;
        Vector3 prefDir = toReq.sqrMagnitude > 0.01f ? toReq.normalized : Vector3.forward;

        for (int i = 0; i < maxWorkers; i++){
            if (occupied[i]) continue;

            // Evenly distribute slots on a circle
            float ang = (i / (float)maxWorkers) * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));

            // Choose slot whose direction matches requester approach
            float d = Vector3.Dot(dir, prefDir);
            if (d > bestDot){ bestDot = d; best = i; }
        }

        if (best == -1) return false;

        occupied[best] = true;
        slotIndex = best;

        float pad = Mathf.Max(1f, radialPadding);
        float radius = gatherRadius * pad;

        float angle = (best / (float)maxWorkers) * Mathf.PI * 2f;
        Vector3 dirBest = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        worldPos = transform.position + dirBest * radius;

        // Keep y from drifting: sample ground
        if (UnityEngine.AI.NavMesh.SamplePosition(worldPos, out var hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas)){
            worldPos = hit.position;
        }
        return true;
    }

    public void ReleaseWorker(int slotIndex){
        if (occupied == null) return;
        if (slotIndex >= 0 && slotIndex < occupied.Length){
            occupied[slotIndex] = false;
        }
    }

    public bool Take(int amt){
        if (amount <= 0) return false;
        int take = Mathf.Min(amt, amount);
        amount -= take;
        return take > 0;
    }
}
