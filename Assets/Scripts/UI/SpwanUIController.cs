using UnityEngine;
using UnityEngine.UI;

public class SpawnUIController : MonoBehaviour {
    public SelectionManager selection;   // drag from scene
    public GameObject spawnButtonRoot;   // the whole Button object (not just Button)
    public Button spawnButton;           // optional; if assigned we'll also toggle interactable

    void Update(){
        var sp = FindSelectedSpawner();

        // Show only when a building-with-spawner is selected
        if (spawnButtonRoot) spawnButtonRoot.SetActive(sp != null);

        // If visible, also control interactable (optional)
        if (spawnButton) spawnButton.interactable = (sp != null);
    }

    public void SpawnClicked(){
        var sp = FindSelectedSpawner();
        if (sp) sp.SpawnOne();
    }

    Spawner FindSelectedSpawner(){
        if (selection == null || selection.Current == null) return null;
        foreach (var sel in selection.Current){
            var sp = sel.GetComponent<Spawner>();
            if (sp) return sp;          // first selected building with Spawner
        }
        return null;
    }
}
