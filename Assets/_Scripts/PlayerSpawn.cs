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
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
        if (player != null) { StartCoroutine(DieCoroutine()); }
        //PhotonNetwork.Destroy(player);
        //SpawnPlayer();
    }

    private IEnumerator DieCoroutine()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        player = null;

        UIManager.instance.deathPanel.SetActive(true);
        yield return new WaitForSeconds(5f);
        UIManager.instance.deathPanel.SetActive(false);
        if (MatchManager.instance.state == MatchManager.GameState.Playing && player == null)
        {
            SpawnPlayer();
        }
    }
}
