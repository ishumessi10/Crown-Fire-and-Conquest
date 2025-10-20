using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProductionOption {
    public string displayName;
    public GameObject prefab;     // unit prefab to spawn
    public float buildTime = 5f;  // seconds to produce
    public Sprite icon;           // optional: UI icon
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
    public int QueueCount => queue.Count + (current!=null ? 1 : 0);
    public float CurrentProgress01 => current==null ? 0f : 1f - (current.remaining / Mathf.Max(0.0001f, current.opt.buildTime));
    public Sprite CurrentIcon => current?.opt.icon;

    void Update(){
        if (current == null){
            if (queue.Count > 0){
                current = queue.Dequeue();
            } else return;
        }

        current.remaining -= Time.deltaTime;
        if (current.remaining <= 0f){
            Produce(current.opt);
            current = null;
        }
    }

    public bool Enqueue(int optionIndex){
        if (optionIndex < 0 || optionIndex >= options.Count) return false;
        if (QueueCount >= maxQueue) return false;
        var q = new Queued(options[optionIndex]);
        if (current == null) current = q;
        else queue.Enqueue(q);
        return true;
    }

    public bool CancelFront(){ // cancels current
        if (current == null) return false;
        current = null;
        return true;
    }

    public bool CancelLast(){ // pop from end of queue
        if (queue.Count == 0) return false;
        // rebuild without last
        var temp = new List<Queued>(queue);
        temp.RemoveAt(temp.Count-1);
        queue.Clear();
        foreach (var q in temp) queue.Enqueue(q);
        return true;
    }

    public List<Sprite> GetQueueIcons(){ // for UI preview
        var list = new List<Sprite>();
        if (current != null) list.Add(current.opt.icon);
        foreach (var q in queue) list.Add(q.opt.icon);
        return list;
    }

    void Produce(ProductionOption opt){
        var pos = spawnAnchor ? spawnAnchor.position : transform.position + transform.forward * 1.5f;
        var go = Instantiate(opt.prefab, pos, Quaternion.identity);

        var mover = go.GetComponent<UnitMover>();
        if (mover && rally && rally.HasPoint){
            mover.IssueMove(rally.Point);
        }
    }
}
