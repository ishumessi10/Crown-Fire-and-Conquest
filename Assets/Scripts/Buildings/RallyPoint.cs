using UnityEngine;

public class RallyPoint : MonoBehaviour {
    public LayerMask groundMask;
    public Transform marker; // optional: a small flag/marker object

    public Vector3 Point { get; private set; }
    public bool HasPoint { get; private set; }

    public void SetPoint(Vector3 p){
        Point = p; HasPoint = true;
        if (marker){
            marker.gameObject.SetActive(true);
            marker.position = p + Vector3.up * 0.02f;
        }
    }
}
