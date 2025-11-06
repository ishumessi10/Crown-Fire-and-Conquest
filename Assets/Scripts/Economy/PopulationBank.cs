using UnityEngine;

public class PopulationBank : MonoBehaviour {
    public static PopulationBank I;

    [Header("Population")]
    public int current = 0;   // current used pop
    public int max = 10;      // max pop cap

    void Awake(){
        if (I != null && I != this){ Destroy(gameObject); return; }
        I = this;
    }

    public bool HasRoom(int need = 1) => current + need <= max;

    // Call when a unit finishes spawning
    public void OnUnitSpawned(int size = 1){
        current += size;
        if (current < 0) current = 0;
    }

    // Call when a unit dies/despawns (optional)
    public void OnUnitLost(int size = 1){
        current -= size;
        if (current < 0) current = 0;
    }

    // Houses/techs call this to increase cap
    public void AddCap(int add){
        max += add;
        if (max < 0) max = 0;
    }
}
