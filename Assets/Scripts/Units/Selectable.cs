using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Selectable : MonoBehaviour {
    public bool isSelected;
    public void SetSelected(bool v){
        isSelected = v;
        var r = GetComponentInChildren<Renderer>();
        if (r && r.material.HasProperty("_Color"))
            r.material.color = v ? Color.yellow : Color.white;
    }
}
