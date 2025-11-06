using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProductionOption {
    public string displayName;
    public GameObject prefab;
    public float buildTime = 5f;
    public Sprite icon;
    public List<ResourceCost> cost = new List<ResourceCost>(); // costs paid on enqueue
}

public class ProductionBuilding : MonoBehaviour {
    [Header("Catalog (set per-building)")]
    public List<ProductionOption> options = new List<ProductionOption>();

    [Header("Spawn")]
    public Transform spawnAnchor;     // where unit appears
    public RallyPoint rally;          // optional

    [Header("Queue")]
    public int maxQueue = 8;

    class Queued {
        public ProductionOption opt;
        public float remaining;
        public Queued(ProductionOption o){ opt = o; remaining = o.buildTime; }
    }

    readonly Queue<Queued> queue = new Queue<Queued>();
    Queued current;

    // Public read for UI
    public IReadOnlyCollection<ProductionOption> Catalog => options;
    public int QueueCount => queue.Count + (current != null ? 1 : 0);
    public float CurrentProgress01 => current == null ? 0f : 1f - (current.remaining / Mathf.Max(0.0001f, current.opt.buildTime));
    public Sprite CurrentIcon => current?.opt.icon;

    void Update(){
        if (current == null){
            if (queue.Count > 0){
                current = queue.Dequeue();
            } else {
                return;
            }
        }

        current.remaining -= Time.deltaTime;
        if (current.remaining <= 0f){
            // Try to produce; if it can’t (e.g., pop cap), we’ll retry shortly.
            if (TryProduce(current.opt)){
                current = null;
            } else {
                // Stall a little and try again while waiting for room.
                current.remaining = 0.1f;
            }
        }
    }

    public bool Enqueue(int optionIndex){
        // Pop cap check on enqueue (QoL). We’ll also guard at spawn time.
        if (PopulationBank.I && !PopulationBank.I.HasRoom(1)) {
            // TODO: flash "Population cap reached"
            return false;
        }
        if (optionIndex < 0 || optionIndex >= options.Count) return false;
        if (QueueCount >= maxQueue) return false;

        var opt = options[optionIndex];
        // Pay upfront
        if (!ResourceBank.I || !ResourceBank.I.TrySpend(opt.cost)) {
            // TODO: flash "Not enough resources"
            return false;
        }

        var q = new Queued(opt);
        if (current == null) current = q;
        else queue.Enqueue(q);
        return true;
    }

    public bool CancelFront(){ // cancels current and refunds full cost
        if (current == null) return false;
        if (ResourceBank.I != null) ResourceBank.I.Refund(current.opt.cost);
        current = null;
        return true;
    }

    public bool CancelLast(){
        if (queue.Count == 0) return false;
        var temp = new List<Queued>(queue);
        var last = temp[temp.Count - 1];
        temp.RemoveAt(temp.Count - 1);
        queue.Clear();
        foreach (var q in temp) queue.Enqueue(q);
        if (ResourceBank.I != null) ResourceBank.I.Refund(last.opt.cost);
        return true;
    }

    public List<Sprite> GetQueueIcons(){ // for UI preview
        var list = new List<Sprite>();
        if (current != null) list.Add(current.opt.icon);
        foreach (var q in queue) list.Add(q.opt.icon);
        return list;
    }

    // Try to spawn the unit; return true on success, false if postponed (e.g., no pop room)
    bool TryProduce(ProductionOption opt){
        // Guard pop cap at spawn time (queue may have been created earlier)
        if (PopulationBank.I && !PopulationBank.I.HasRoom(1)){
            return false; // postpone; Update() will retry
        }

        var pos = spawnAnchor ? spawnAnchor.position : transform.position + transform.forward * 1.5f;
        var go = Instantiate(opt.prefab, pos, Quaternion.identity);

        // Count population
        PopulationBank.I?.OnUnitSpawned(1);

        // Send to rally if set
        var mover = go.GetComponent<UnitMover>();
        if (mover && rally && rally.HasPoint){
            mover.IssueMove(rally.Point);
        }
        return true;
    }
}
