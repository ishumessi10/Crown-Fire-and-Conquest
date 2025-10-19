using UnityEngine;

public class Health : MonoBehaviour {
    public float maxHP = 50f;
    public float currentHP;

    void Awake(){ currentHP = maxHP; }

    public bool IsAlive => currentHP > 0f;

    public void Take(float dmg){
        if (!IsAlive) return;
        currentHP -= Mathf.Max(0f, dmg);
        if (currentHP <= 0f){
            currentHP = 0f;
            // simple death: disable object
            gameObject.SetActive(false);
            // Debug.Log($"[Death] {name}");
        }
    }
}
