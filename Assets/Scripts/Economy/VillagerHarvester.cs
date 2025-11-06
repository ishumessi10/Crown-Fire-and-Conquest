using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnitMover))]
public class VillagerHarvester : MonoBehaviour {
    public int carryCapacity = 20;
    public float depositTime = 0.6f;

    UnitMover mover;
    ResourceNode node;
    DropoffPoint drop;

    ResourceType carryingType;
    int carried;
    bool active;

    // NEW: reserved slot + position
    int slotIndex = -1;
    Vector3 workPos;

    int dropSlotIndex = -1;
    Vector3 dropSlotPos;

    void Awake(){ mover = GetComponent<UnitMover>(); }

    public void IssueGather(ResourceNode n){
        Cancel();
        node = n;
        if (!node) return;

        carryingType = node.type;
        carried = 0;
        active = true;

        TryReserveAndGoToSlot();
    }

    public void Cancel(){
        active = false;
        CancelInvoke();
        if (node){
            node.ReleaseWorker(slotIndex);
        }
        slotIndex = -1;
        node = null;
        drop = null;
        carried = 0;
    }

    void TryReserveAndGoToSlot(){
        if (!node){ active = false; return; }

        // Reserve a specific slot around the node
        if (!node.TryReserveWorker(out slotIndex, out workPos, transform.position)){
            // Node full; retry soon or bail
            Invoke(nameof(TryReserveAndGoToSlot), 0.5f);
            return;
        }

        mover.IssueMove(workPos);
        mover.SetArrivalCallback(StartHarvest);
    }

    void StartHarvest(){
        if (!active || !node) return;

        // If we somehow didn’t get a valid slot, try again
        float dist = Vector3.Distance(transform.position, workPos);
        if (slotIndex < 0 || dist > 1.0f){
            node.ReleaseWorker(slotIndex);
            slotIndex = -1;
            TryReserveAndGoToSlot();
            return;
        }

        // Begin periodic harvesting
        InvokeRepeating(nameof(HarvestTick), node.secondsPerTick, node.secondsPerTick);
    }

    void HarvestTick(){
        if (!active || !node){ StopHarvest(); return; }

        if (node.amount <= 0){
            StopHarvest();
            FindNewNodeOrStop();
            return;
        }

        if (node.Take(node.yieldPerTick)){
            carried += node.yieldPerTick;
            if (carried >= carryCapacity){
                StopHarvest();
                FindDropoff();
            }
        }
    }

    void StopHarvest()
    {
        CancelInvoke(nameof(HarvestTick));
        if (node)
        {
            node.ReleaseWorker(slotIndex);
        }
        slotIndex = -1;
    }

    // void FindDropoff(){
    //     drop = FindClosestDropoff(carryingType);
    //     if (!drop){ Cancel(); return; }
    //     mover.IssueMove(drop.transform.position);
    //     mover.SetArrivalCallback(Deposit);
    // }

    // void Deposit(){
    //     if (!active){ return; }
    //     Invoke(nameof(DoDeposit), depositTime);
    // }

    // void DoDeposit(){
    //     if (ResourceBank.I != null && carried > 0){
    //         ResourceBank.I.Add(carryingType, carried);
    //     }
    //     carried = 0;
    //     ResumeAfterDeposit();
    // }
    
    void FindDropoff(){
        drop = FindClosestDropoff(carryingType);
        if (!drop){ Cancel(); return; }

        // Try to reserve a slot; if full, retry soon
        if (!drop.TryReserveSlot(out dropSlotIndex, out dropSlotPos, transform.position)){
            Invoke(nameof(FindDropoff), 0.4f);
            return;
        }

        mover.IssueMove(dropSlotPos);
        mover.SetArrivalCallback(Deposit);
    }

    void Deposit(){
        if (!active){ ReleaseDropSlot(); return; }
        // small delay to simulate unload
        Invoke(nameof(DoDeposit), depositTime);
    }

    void DoDeposit(){
        if (ResourceBank.I != null && carried > 0){
            ResourceBank.I.Add(carryingType, carried);
        }
        carried = 0;
        ReleaseDropSlot();
        ResumeAfterDeposit();
    }

    void ReleaseDropSlot(){
        if (drop){
            drop.ReleaseSlot(dropSlotIndex);
        }
        dropSlotIndex = -1;
    }

    void ResumeAfterDeposit(){
        if (!active) return;

        // Go back to the same node if it still has resources
        if (node && node.amount > 0){
            TryReserveAndGoToSlot();
        } else {
            FindNewNodeOrStop();
        }
    }

    void FindNewNodeOrStop(){
        // For now, stop. (Later: search nearest same-type node in range)
        Cancel();
    }

    DropoffPoint FindClosestDropoff(ResourceType t){
        var drops = GameObject.FindObjectsOfType<DropoffPoint>();
        DropoffPoint best = null; float bestD = float.PositiveInfinity;
        foreach (var d in drops){
            if (!d.Accepts(t)) continue;
            float dsq = (d.transform.position - transform.position).sqrMagnitude;
            if (dsq < bestD){ bestD = dsq; best = d; }
        }
        return best;
    }
}
