using UnityEngine;
using UnityEngine.AI;

public class Constructable : MonoBehaviour
{
    [Header("Build Settings")]
    [Tooltip("Seconds for ONE villager to fully build this.")]
    public float baseBuildTime = 10f;

    [Tooltip("Max number of villagers counted for speed.")]
    public int maxBuilders = 5;

    NavMeshObstacle obstacle;

    float buildProgress01;  // 0 → 1
    int currentBuilders;
    bool isBuilt;

    void Awake()
    {
        obstacle = GetComponentInChildren<NavMeshObstacle>();

        if (obstacle != null)
            obstacle.enabled = false;   // only on when finished

        buildProgress01 = 0f;
        isBuilt = false;

        Debug.Log($"[Constructable] {name} Awake, obstacle disabled");
    }

    /// <summary>
    /// Called by ObjectPlacer right after instantiate.
    /// </summary>
    public void ConstructableWasPlaced()
    {
        buildProgress01 = 0f;
        isBuilt = false;

        if (obstacle != null)
            obstacle.enabled = false;

        Debug.Log($"[Constructable] {name} placed; waiting for builders");
    }

    void Update()
    {
        if (isBuilt) return;
        if (currentBuilders <= 0) return;

        float speedPerSecond = currentBuilders / baseBuildTime;
        buildProgress01 += speedPerSecond * Time.deltaTime;

        Debug.Log($"[Constructable] {name} progress={buildProgress01:F2}, builders={currentBuilders}");

        if (buildProgress01 >= 1f)
        {
            FinishConstruction();
        }
    }

    void FinishConstruction()
    {
        buildProgress01 = 1f;
        isBuilt = true;

        if (obstacle != null)
            obstacle.enabled = true;

        Debug.Log($"[Constructable] {name} FINISHED, obstacle enabled");
    }

    // -------- called by villagers --------
    public void AddBuilder()
    {
        if (isBuilt) return;
        currentBuilders = Mathf.Clamp(currentBuilders + 1, 0, maxBuilders);
        Debug.Log($"[Constructable] {name} builder++ → {currentBuilders}");
    }

    public void RemoveBuilder()
    {
        if (isBuilt) return;
        currentBuilders = Mathf.Max(0, currentBuilders - 1);
        Debug.Log($"[Constructable] {name} builder-- → {currentBuilders}");
    }

    public float GetProgress01() => buildProgress01;
    public bool IsBuilt() => isBuilt;
}
