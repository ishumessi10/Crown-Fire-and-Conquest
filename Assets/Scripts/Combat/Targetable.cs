using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Targetable : MonoBehaviour {
    public Health health;

    void Reset(){
        health = GetComponent<Health>();
    }
}
