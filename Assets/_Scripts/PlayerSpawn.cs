using Photon.Pun;
using UnityEngine;
using System.Collections;
public class PlayerSpawn : MonoBehaviour
{
    public static PlayerSpawn instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] GameObject playerPrefab;
    private GameObject player;

    [SerializeField] GameObject deathEffect;

    private void Start()
    {
        if (PhotonNetwork.IsConnected) { SpawnPlayer(); }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetRandomSpawnPositions();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void PlayerDie(string dName)
    {
        
        UIManager.instance.deathText.text = "killed by " + dName;
        if (player != null) { StartCoroutine(DieCoroutine()); }
        //PhotonNetwork.Destroy(player);
        //SpawnPlayer();
    }

    private IEnumerator DieCoroutine()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIManager.instance.deathPanel.SetActive(true);
        yield return new WaitForSeconds(3f);
        UIManager.instance.deathPanel.SetActive(false);
        SpawnPlayer();
    }
}
