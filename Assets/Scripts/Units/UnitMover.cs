using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMover : MonoBehaviour {
    NavMeshAgent agent;
    readonly Queue<Vector3> queue = new Queue<Vector3>();
    const float arriveEpsilon = 0.2f;

    bool holdPosition; // <- new

    void Awake(){ agent = GetComponent<NavMeshAgent>(); }

    public bool IsHolding => holdPosition;

    public void SetHold(bool on){
        holdPosition = on;
        if (on){
            agent.ResetPath();
            agent.isStopped = true;
            queue.Clear();
            // Debug.Log($"[Hold] {name} hold ON");
        } else {
            agent.isStopped = false;
            // Debug.Log($"[Hold] {name} hold OFF");
        }
    }

    public void StopNow(){
        queue.Clear();
        agent.ResetPath();
        agent.isStopped = false; // still allow immediate moves after stop
        // Debug.Log($"[Stop] {name}");
    }

    public void IssueMove(Vector3 dest){
        if (holdPosition) return;    // ignore commands while holding
        queue.Clear();
        agent.isStopped = false;
        SetDest(dest);
    }

    public void QueueMove(Vector3 dest){
        if (holdPosition) return;
        if (!agent.enabled) return;

        if (!agent.hasPath && queue.Count == 0){
            agent.isStopped = false;
            SetDest(dest);
        } else {
            queue.Enqueue(dest);
        }
    }

    void Update(){
        if (holdPosition || !agent.enabled) return;

        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, arriveEpsilon)){
            if (queue.Count > 0){
                SetDest(queue.Dequeue());
            }
        }
    }

    void SetDest(Vector3 p){
        agent.SetDestination(p);
        // Debug.Log($"[Move] {name} -> {p}");
    }
}
