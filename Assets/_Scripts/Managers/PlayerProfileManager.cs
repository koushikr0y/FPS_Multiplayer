using UnityEngine;
using TMPro;
using Photon.Pun;
public class PlayerProfileManager : MonoBehaviour
{
    [SerializeField] TMP_InputField playerNameInputField;

    private void Start()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            playerNameInputField.text = PlayerPrefs.GetString("username");
            PhotonNetwork.NickName = PlayerPrefs.GetString("username");
        }
        else
        {
            //take random name
            playerNameInputField.text = "Player " + Random.Range(0, 100).ToString();
            SetUserNameChanged();
        }

    }

    public void SetUserNameChanged()
    {
        PhotonNetwork.NickName = playerNameInputField.text;
        PlayerPrefs.SetString("username", playerNameInputField.text);
    }
}
