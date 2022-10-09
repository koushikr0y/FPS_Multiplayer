//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    public Transform[] spawnPoints;

    private void Awake()
    {
        if (instance != null) { Destroy(gameObject); }
        else { instance = this; }
    }
    private void Start()
    {
        foreach(Transform sp in spawnPoints)
        {
            sp.gameObject.SetActive(false);
        }
    }
    public Transform GetRandomSpawnPositions()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
