using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }

    //gamestate
    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    public int killsToWin = 3;
    public float waitAfterEnding = 5f;
    public GameState state = GameState.Waiting;

    [SerializeField] bool perpetual; //check after complete one game continue to next game 
    //timer
    public float matchLength = 10f;
    private float currentMatchTime;
    private float sendTimer;
    enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat,
        NextMatch,
        SyncTimer
    }
    [SerializeField] List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    [SerializeField] List<Leaderboard> leaderBoardPlayers =  new List<Leaderboard>();

    private void Start()
    {
#if UNITY_EDITOR
        if (!PhotonNetwork.IsConnected) { SceneManager.LoadScene(0); }
#endif
        NewPlayerSend(PhotonNetwork.NickName);
        state = GameState.Playing;
        SetUpTimer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && state!=GameState.Ending)
        {
            if (UIManager.instance.leaderBoardPannel.activeInHierarchy)
            { UIManager.instance.leaderBoardPannel.SetActive(false); }
            else
            {
                ShowLeaderBoard();
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (currentMatchTime > 0f && state == GameState.Playing)
            {
                currentMatchTime -= Time.deltaTime;
                if (currentMatchTime <= 0f)
                {
                    currentMatchTime = 0f;
                    state = GameState.Ending;
                        ListPlayerSend();
                        StateCheck();
                }
                TimerDisplay();
                sendTimer -= Time.deltaTime;
                if(sendTimer <= 0)
                {
                    sendTimer += 1f;
                }
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200) 
        {
            EventCodes _event = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (_event)
            {
                case EventCodes.NewPlayer:
                    NewPlayerRecieve(data);
                    break;

                case EventCodes.ListPlayers:
                    ListPlayerRecieve(data);
                    break;

                case EventCodes.UpdateStat:
                    UpdateStatsReceive(data);
                    break;

                case EventCodes.NextMatch:
                    NextMatchReceive();
                    break;

                case EventCodes.SyncTimer:
                    TimerReceive(data);
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this); //add to list when event call back happen
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this); 
    }

    private void NewPlayerSend(string _username) 
    { 
        object[] package = new object[4];
        package[0] = _username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer, 
            package, 
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, 
            new SendOptions { Reliability = true }
            ); 
    }
    private void NewPlayerRecieve(object[] dataRecieved) 
    {
        PlayerInfo _infoPlayer = new PlayerInfo((string)dataRecieved[0], (int)dataRecieved[1], (int)dataRecieved[2], (int)dataRecieved[3]);
        allPlayers.Add(_infoPlayer);
        ListPlayerSend();
    }
    private void ListPlayerSend() 
    {
        object[] package = new object[allPlayers.Count + 1];
        package[0] = state;
        for(int i = 0; i < allPlayers.Count; i++)
        {
            object[] pb = new object[4];

            pb[0] = allPlayers[i].name;
            pb[1] = allPlayers[i].actor;
            pb[2] = allPlayers[i].kills;
            pb[3] = allPlayers[i].deaths;

            package[i+1] = pb;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    private void ListPlayerRecieve(object[] dataRecieved) 
    {
        allPlayers.Clear();
        state = (GameState)dataRecieved[0];
        for(int i = 1; i < dataRecieved.Length; i++)
        {
            object[] pb = (object[])dataRecieved[i];

            PlayerInfo playerInfo = new PlayerInfo(
                (string)pb[0],
                (int)pb[1],
                (int)pb[2],
                (int)pb[3]
                );

            allPlayers.Add(playerInfo);

            if(PhotonNetwork.LocalPlayer.ActorNumber == playerInfo.actor) 
            {
                index = i-1;
            }
        }
        StateCheck();
    }
    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange) 
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    private void UpdateStatsReceive(object[] dataRecieved) 
    {
        int actor = (int)dataRecieved[0];
        int statType = (int)dataRecieved[1];
        int amount = (int)dataRecieved[2];

        for(int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    case 0:
                        allPlayers[i].kills += amount;
                        break;

                    case 1:
                        allPlayers[i].deaths += amount;
                        break;
                }
                if (i == index) //if the player start value changes
                {
                    UpdateStatsDisplay();
                }

                if (UIManager.instance.leaderBoardPannel.activeInHierarchy) { ShowLeaderBoard(); }
                break;
            }
        }
        ScoreCheck();
    }

    private void UpdateStatsDisplay()
    {
        if (allPlayers.Count > index)
        {
            UIManager.instance.killsText.text = "KILLS: " + allPlayers[index].kills;
            UIManager.instance.deathsText.text = "DEATHS: " + allPlayers[index].deaths;
        }
        else
        {
            UIManager.instance.killsText.text = "KILLS: 0";
            UIManager.instance.deathsText.text = "DEATHS: 0";
        }
    }

    private void ShowLeaderBoard()
    {
        UIManager.instance.leaderBoardPannel.SetActive(true);

        foreach(Leaderboard lbp in leaderBoardPlayers)
        {
            Destroy(lbp.gameObject);
        }
        leaderBoardPlayers.Clear();
        UIManager.instance.lboardPlayerInfo.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        //display
        foreach(PlayerInfo player in allPlayers)
        {
            Leaderboard newPlayerDisplay = Instantiate(UIManager.instance.lboardPlayerInfo,
                UIManager.instance.lboardPlayerInfo.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);

            leaderBoardPlayers.Add(newPlayerDisplay);
        }
    }

    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> psort = new List<PlayerInfo>();

        while (psort.Count < players.Count) 
        {
            int highestKill = -1;
            PlayerInfo selectedPlayer = players[0];
            foreach (PlayerInfo player in players)
            {
                if (!psort.Contains(player))
                {
                    if (player.kills > highestKill)
                    {
                        selectedPlayer = player;
                        highestKill = player.kills;
                    }
                }
            }

            psort.Add(selectedPlayer);
        }

        return psort;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom(); 
        SceneManager.LoadScene(0);
    }

    private void ScoreCheck() 
    {
        bool _winner = false;
        foreach(PlayerInfo player in allPlayers) 
        { 
            if(player.kills >= killsToWin && killsToWin>0)
            {
                _winner = true;
                break;
            }
        }
        if (_winner)
        {
            if(PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayerSend();
            }
        }
    }

    private void StateCheck()
    {
        if(state == GameState.Ending)
        {
            EndGame();
        }
    }
    private void EndGame()
    {
        state = GameState.Ending;
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        UIManager.instance.endScreenPannel.SetActive(true);
        ShowLeaderBoard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(EndGameMenu());
    }

    private IEnumerator EndGameMenu()
    {
        yield return new WaitForSeconds(waitAfterEnding);
        if (!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            //start new match
            if (PhotonNetwork.IsMasterClient)
            {
                NextMatchSend();
            }
        }
    }

    private void NextMatchSend() 
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    private void NextMatchReceive() 
    {
        state = GameState.Playing;
        UIManager.instance.endScreenPannel.SetActive(false);
        UIManager.instance.leaderBoardPannel.SetActive(false);

        foreach(PlayerInfo player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }

        UpdateStatsDisplay();
        PlayerSpawn.instance.SpawnPlayer();
        SetUpTimer();
    }

    private void SetUpTimer()
    {
        if(matchLength > 0)
        {
            currentMatchTime = matchLength;
            TimerDisplay();
        }
    }

    private void TimerDisplay()
    {
        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);
        UIManager.instance.timerText.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");

    }

    private void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.SyncTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    private void TimerReceive(object[] dataReceived)
    {
        currentMatchTime = (int)dataReceived[0];
        state = (GameState)dataReceived[1];

        TimerDisplay();
    }
}

[System.Serializable]
class PlayerInfo
{
    public string name;
    public int actor,kills,deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }
}
