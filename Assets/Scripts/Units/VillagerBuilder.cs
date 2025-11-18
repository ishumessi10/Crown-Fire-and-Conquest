using UnityEngine;

[RequireComponent(typeof(UnitMover))]
public class VillagerBuilder : MonoBehaviour
{
    UnitMover mover;
    Constructable currentTarget;
    bool isBuilding;

    void Awake()
    {
        mover = GetComponent<UnitMover>();
        Debug.Log($"[VillagerBuilder] Awake on {name}");
    }

public void OrderBuild(Constructable target)
{
    // Stop any previous job
    StopBuilding();

    currentTarget = target;

    Debug.Log($"[VillagerBuilder] {name} ORDER BUILD {target.name}");

    // 1. Move towards the building
    mover.IssueMove(target.transform.position);

    // 2. THEN set the arrival callback, so IssueMove doesn't clear it
    mover.SetArrivalCallback(OnArrivedAtBuildSite);
}


    void OnArrivedAtBuildSite()
    {
        if (currentTarget == null) return;
        if (currentTarget.IsBuilt()){
            Debug.Log($"[VillagerBuilder] {name} arrived but {currentTarget.name} already built");
            currentTarget = null;
            return;
        }

        isBuilding = true;
        currentTarget.AddBuilder();
        Debug.Log($"[VillagerBuilder] {name} started building {currentTarget.name}");
    }

    public void StopBuilding()
    {
        if (isBuilding && currentTarget != null)
        {
            currentTarget.RemoveBuilder();
            Debug.Log($"[VillagerBuilder] {name} stopped building {currentTarget.name}");
        }

        isBuilding = false;
        currentTarget = null;

        mover.SetArrivalCallback(null);
    }

    void OnDisable()
    {
        StopBuilding();
    }

    public bool IsBuilding => isBuilding;
}
