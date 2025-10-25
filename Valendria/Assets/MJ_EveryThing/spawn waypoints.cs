using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private int spawnCount = 10;

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnPrefab();
        }
    }

    void SpawnPrefab()
    {
        float randomX = Random.Range(-50f, 50f);
        float randomZ = Random.Range(-50f, 50f);
        Vector3 spawnPos = new Vector3(randomX, 1f, randomZ);

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }
}