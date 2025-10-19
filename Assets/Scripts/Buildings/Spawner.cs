using UnityEngine;

public class Spawner : MonoBehaviour {
    public GameObject unitPrefab;        // Villager prefab
    public Transform spawnAnchor;        // where units appear (e.g., in front of building)
    public RallyPoint rally;             // same building's RallyPoint

    public void SpawnOne(){
        var pos = spawnAnchor ? spawnAnchor.position : transform.position + transform.forward * 1.5f;
        var go = Instantiate(unitPrefab, pos, Quaternion.identity);
        var mover = go.GetComponent<UnitMover>();
        if (mover && rally && rally.HasPoint){
            mover.IssueMove(rally.Point);
        }
    }
}
