using System;                      // <-- for Action
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMover : MonoBehaviour {
    NavMeshAgent agent;
    readonly Queue<Vector3> queue = new Queue<Vector3>();
    const float arriveEpsilon = 0.2f;

    bool holdPosition;

    Action onArrive;               // <-- one-shot arrival callback

    void Awake(){ agent = GetComponent<NavMeshAgent>(); }

    public bool IsHolding => holdPosition;

    public void SetHold(bool on){
        holdPosition = on;
        if (on){
            agent.ResetPath();
            agent.isStopped = true;
            queue.Clear();
            onArrive = null;
        } else {
            agent.isStopped = false;
        }
    }

    public void StopNow(){
        queue.Clear();
        onArrive = null;
        agent.ResetPath();
        agent.isStopped = false; // allow immediate new commands
    }

    public void IssueMove(Vector3 dest){
        if (holdPosition || !agent.enabled) return;
        queue.Clear();
        onArrive = null;
        agent.isStopped = false;
        SetDest(dest);
    }

    public void QueueMove(Vector3 dest){
        if (holdPosition || !agent.enabled) return;

        if (!agent.hasPath && queue.Count == 0){
            agent.isStopped = false;
            SetDest(dest);
        } else {
            queue.Enqueue(dest);
        }
    }

    public void SetArrivalCallback(Action cb){
        onArrive = cb;             // one-shot; cleared on trigger
    }

    void Update(){
        if (holdPosition || !agent.enabled) return;

        // Only when we're basically at the destination and not computing a path
        bool arrived = !agent.pathPending &&
                       agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, arriveEpsilon);

        if (arrived){
            // Fire any one-shot arrival callback first
            var cb = onArrive; onArrive = null;
            cb?.Invoke();

            // Then continue queued waypoints if any
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
