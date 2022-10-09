using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Launch : MonoBehaviourPunCallbacks
{

    public static Launch instance;
    private void Awake()
    {
        instance = this;
    }

    #region
    public GameObject loadingScreen;
    public TMP_Text loadingText;
    public GameObject menuButton;
    public GameObject createRoomPannel;
    public TMP_InputField roomNameIF;

    public GameObject Roompannel;
    public TMP_Text roomNameText, playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();
    public GameObject errorPannel;
    public TMP_Text errorText;

    public GameObject roomBrowserPannel;
    public RoomButton roomButton;
    [SerializeField] private List<RoomButton> allRoomButtons = new List<RoomButton>();

    public GameObject nameInputScreen;
    public TMP_InputField nameInput;
    public static bool hasSetNick;

    public string levelToPlay;
    public GameObject startButton;

    public GameObject roomTestButton;

    public string[] allMaps;
    public bool changeMapBetweenRounds = true;
    #endregion

    private void Start()
    {
        CloseMenu();
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to network...";

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CloseMenu()
    {
        loadingScreen.SetActive(false);
        menuButton.SetActive(false);
        createRoomPannel.SetActive(false);
        Roompannel.SetActive(false);
        errorPannel.SetActive(false);
        roomBrowserPannel.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void OpenRoom()
    {
        CloseMenu();
        createRoomPannel.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameIF.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(roomNameIF.text, options);

            CloseMenu();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);
        }
    }

    public override void OnJoinedRoom()
    {
        CloseMenu();
        Roompannel.SetActive(true);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerLabel);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenu();
        errorText.text = "FAILED TO CREATE ROOM" + message;
        errorPannel.SetActive(true);
    }

    public void CloseErrorPannel()
    {
        CloseMenu();
        menuButton.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        CloseMenu(); menuButton.SetActive(true);
    }

    public void OpenRoomBrowser()
    {
        CloseMenu();
        roomBrowserPannel.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenu();
        menuButton.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        allRoomButtons.Clear();

        roomButton.gameObject.SetActive(false);

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                RoomButton newButton = Instantiate(roomButton, roomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                allRoomButtons.Add(newButton);
            }
        }
    }

    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenu();
        loadingText.text = "JOINING ROOM...";
        loadingScreen.SetActive(true);
    }
    public override void OnJoinedLobby()
    {
        CloseMenu();
        menuButton.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (!hasSetNick)
        {
            CloseMenu();
            nameInputScreen.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }
    public void SetNickname()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;

            PlayerPrefs.SetString("playerName", nameInput.text);

            CloseMenu();
            menuButton.SetActive(true);

            hasSetNick = true;
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }
}
