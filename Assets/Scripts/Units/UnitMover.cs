using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMover : MonoBehaviour {
    NavMeshAgent agent;
    void Awake(){ agent = GetComponent<NavMeshAgent>(); }
    public void IssueMove(Vector3 dest){
        if (!agent.enabled) return;
        agent.SetDestination(dest);
        Debug.Log($"[Move] {name} -> {dest}");
    }
}
