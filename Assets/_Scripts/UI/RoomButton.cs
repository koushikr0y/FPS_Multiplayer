using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public TMP_Text buttonText;
    public RoomInfo roomInfo;

    public void SetButtonDetails(RoomInfo inputInfo)
    {
        roomInfo = inputInfo;
        buttonText.text = roomInfo.Name; 
    }

    public void OpenRoom()
    {
        Launch.instance.JoinRoom(roomInfo);
    }
}
