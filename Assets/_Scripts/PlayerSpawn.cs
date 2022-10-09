using Photon.Pun;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public static PlayerSpawn instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] GameObject playerPrefab;
    private GameObject player;

    private void Start()
    {
        if (PhotonNetwork.IsConnected) { SpawnPlayer(); }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetRandomSpawnPositions();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }
}
