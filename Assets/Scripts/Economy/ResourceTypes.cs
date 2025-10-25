using System;
using UnityEngine;

public enum ResourceType { Food, Wood, Fibre, Metal, Gold }

[Serializable]
public struct ResourceCost {
    public ResourceType type;
    public int amount;
}