using UnityEngine;

public class Team : MonoBehaviour {
    [Tooltip("0 = Player, 1 = Enemy, 2+ = others")]
    [Range(0,7)] public int teamId = 0;
}