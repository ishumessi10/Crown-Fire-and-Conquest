using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnitMover))]
public class AttackUnit : MonoBehaviour {
    public float detectRadius = 6f;
    public float attackRange = 1.7f;
    public float attackDamage = 10f;
    public float attackCooldown = 0.8f;
    public LayerMask enemyMask; // set in inspector

    UnitMover mover;
    NavMeshAgent agent;
    Targetable currentTarget;
    float cd;

    void Awake()
    {
        mover = GetComponent<UnitMover>();
        agent = GetComponent<NavMeshAgent>();
    }
    
    bool autoAcquire;                       // <- new

    public bool HasTarget => currentTarget != null;

    public void ClearTarget(){              // <- update: disables aggro
        currentTarget = null;
        autoAcquire = false;
    }

    public void SetAutoAcquire(bool on){    // optional helper
        autoAcquire = on;
        if (!on) currentTarget = null;
    }

    public void IssueAttackTarget(Targetable t){
        currentTarget = t;
        autoAcquire = true;                 // <- add
        mover.IssueMove(t.transform.position);
    }

    public void IssueAttackMove(Vector3 dest){
        currentTarget = null;
        autoAcquire = true;                 // <- add
        mover.IssueMove(dest);
    }


    void Update(){
        cd -= Time.deltaTime;
        if (currentTarget && (!currentTarget.health || !currentTarget.health.IsAlive)){
            currentTarget = null; // target died
        }

        // Auto-acquire if moving and no explicit target
        if (autoAcquire && currentTarget == null){
            var e = FindClosestEnemy();
            if (e) currentTarget = e;
        }

        // Chase target if we have one
        if (currentTarget){
            var tgtPos = currentTarget.transform.position;
            float dist = Vector3.Distance(transform.position, tgtPos);

            if (dist > attackRange * 0.95f){
                // move into range (unless holding)
                if (!mover.IsHolding) agent.SetDestination(tgtPos);
            } else {
                // in range: face + attack
                if (cd <= 0f){
                    cd = attackCooldown;
                    currentTarget.health?.Take(attackDamage);
                    // Debug.Log($"[Attack] {name} -> {currentTarget.name}");
                }
            }
        }
    }

    Targetable FindClosestEnemy(){
        var hits = Physics.OverlapSphere(transform.position, detectRadius, enemyMask, QueryTriggerInteraction.Ignore);
        float best = float.PositiveInfinity; Targetable bestT = null;
        foreach (var h in hits){
            var t = h.GetComponentInParent<Targetable>();
            if (t && t.health && t.health.IsAlive){
                float d = (t.transform.position - transform.position).sqrMagnitude;
                if (d < best){ best = d; bestT = t; }
            }
        }
        return bestT;
    }
}
