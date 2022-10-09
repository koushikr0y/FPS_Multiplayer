using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        
        if (!PhotonNetwork.IsConnected) { PhotonNetwork.ConnectUsingSettings(); }
        MenuManager.instance.OpenMenu("Loading");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        Debug.Log("Connected to master");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("menu");
        Debug.Log("Joined Lobby");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.instance.OpenMenu("Room Menu");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] player = PhotonNetwork.PlayerList;
        
        foreach(Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for(int i=0; i < player.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(player[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        MenuManager.instance.OpenMenu("Error");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for(int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList) 
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("menu");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    //CUSTOM
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text)) { return; }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 5;

        PhotonNetwork.CreateRoom(roomNameInputField.text, roomOptions);
        MenuManager.instance.OpenMenu("Loading");
    }
    //join room
    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("Loading");
    }

    public void LeaveRoom() { PhotonNetwork.LeaveRoom(); }
    //load level
    public void StartGame() { PhotonNetwork.LoadLevel(1); }
}
