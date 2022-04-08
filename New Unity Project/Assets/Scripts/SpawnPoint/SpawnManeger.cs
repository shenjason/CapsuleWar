using UnityEngine;

public class SpawnManeger : MonoBehaviour
{
    public static SpawnManeger Instance;
    public SpawnPoint[] SpawnPoints;

    private void Awake() {
        Instance = this;
        SpawnPoints = GetComponentsInChildren<SpawnPoint>();
    }

    public Transform GetSpawnPoint()
    {
        return SpawnPoints[Random.Range(0, SpawnPoints.Length)].transform;
    }
}
