using UnityEngine;

public class MoveMarker : MonoBehaviour {
    public float lifetime = 1.2f;
    public AnimationCurve scale = AnimationCurve.EaseInOut(0,0.6f,1,1f);
    public AnimationCurve fade  = AnimationCurve.EaseInOut(0,1f,1,0f);
    float t; Material mat; Color baseColor;

    void Awake(){
        var r = GetComponentInChildren<Renderer>();
        if (r) { mat = r.material; baseColor = mat.color; }
    }
    public void ShowAt(Vector3 pos){
        transform.position = pos + Vector3.up * 0.02f;
        t = 0f; gameObject.SetActive(true);
    }
    void Update(){
        t += Time.deltaTime / lifetime;
        transform.localScale = Vector3.one * scale.Evaluate(t);
        if (mat) mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, fade.Evaluate(t));
        if (t >= 1f) gameObject.SetActive(false);
    }
}
