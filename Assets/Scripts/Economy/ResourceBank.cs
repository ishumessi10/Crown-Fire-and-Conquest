using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBank : MonoBehaviour {
    public static ResourceBank I { get; private set; }

    [Header("Starting resources")]
    public int food  = 200;
    public int wood  = 200;
    public int fibre = 0;
    public int metal = 0;
    public int gold  = 200;

    // event when values change: (type, newValue)
    public event Action<ResourceType,int> OnChanged;

    readonly Dictionary<ResourceType,int> store = new Dictionary<ResourceType,int>(5);

    void Awake(){
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        store[ResourceType.Food]  = food;
        store[ResourceType.Wood]  = wood;
        store[ResourceType.Fibre] = fibre;
        store[ResourceType.Metal] = metal;
        store[ResourceType.Gold]  = gold;
    }

    public int Get(ResourceType t) => store.TryGetValue(t, out var v) ? v : 0;

    public void Add(ResourceType t, int amt){
        if (!store.ContainsKey(t)) store[t] = 0;
        store[t] = Mathf.Max(0, store[t] + amt);
        OnChanged?.Invoke(t, store[t]);
    }

    public bool CanAfford(IList<ResourceCost> costs){
        if (costs == null) return true;
        for (int i=0;i<costs.Count;i++){
            var c = costs[i];
            if (Get(c.type) < c.amount) return false;
        }
        return true;
    }

    public bool TrySpend(IList<ResourceCost> costs){
        if (!CanAfford(costs)) return false;
        for (int i=0;i<costs.Count;i++){
            var c = costs[i];
            Add(c.type, -c.amount);
        }
        return true;
    }

    public void Refund(IList<ResourceCost> costs){
        if (costs == null) return;
        for (int i=0;i<costs.Count;i++){
            var c = costs[i];
            Add(c.type, c.amount);
        }
    }
}
