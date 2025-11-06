using UnityEngine;

public class House : MonoBehaviour {
    [Tooltip("How much max population this house provides")]
    public int popProvided = 5;

    bool granted;

    void OnEnable(){
        // Don't grant when this is a placement ghost
        if (GetComponentInParent<PlacementGhost>() != null) return;

        GrantOnce();
    }

    // If you instantiate this directly at runtime, call this manually.
    public void GrantOnce(){
        if (granted) return;
        granted = true;
        if (PopulationBank.I != null){
            PopulationBank.I.AddCap(popProvided);
        } else {
            Debug.LogWarning("[House] No PopulationBank in scene.");
        }
    }
}
