using UnityEngine;

public class ScalePing : MonoBehaviour {
    public float duration = 0.25f;
    public float scale = 1.15f;
    Vector3 baseScale;
    float t;

    void OnEnable(){ baseScale = transform.localScale; t = duration; }
    public void Play(){ t = duration; }
    void Update(){
        if (t > 0f){
            t -= Time.deltaTime;
            float a = 1f - (t / duration);
            float s = Mathf.Lerp(scale, 1f, a);
            transform.localScale = baseScale * s;
        }
    }
}
