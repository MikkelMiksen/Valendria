using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabToSpawn = new();
    [SerializeField] private int spawnCount = 10;

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
                SpawnPrefab(prefabToSpawn[Random.Range(0,2)]);
        }
    }

    void SpawnPrefab(GameObject prefab = null)
    {
        float randomX = Random.Range(-50f, 50f);
        float randomZ = Random.Range(-50f, 50f);
        Vector3 spawnPos = new Vector3(randomX, 1f, randomZ);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}