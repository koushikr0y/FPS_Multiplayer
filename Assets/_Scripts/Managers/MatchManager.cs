//using System.Collections;
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

    enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        ChangeStat
    }
    [SerializeField] List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    private void Start()
    {
#if UNITY_EDITOR
        if (!PhotonNetwork.IsConnected) { SceneManager.LoadScene(0); }
#endif
    }
    private void Update()
    {
        
    }
    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200) 
        {
            EventCodes _event = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;
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

    private void NewPlayerSend() { }
    private void NewPlayerRecieve() { }
    private void ListPlayerSend() { }
    private void ListPlayerRecieve() { }
    private void UpdateStatsSend() { }
    private void UpdateSendReceive() { }
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
