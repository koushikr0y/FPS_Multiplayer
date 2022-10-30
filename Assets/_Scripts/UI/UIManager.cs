//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;
using Photon.Pun;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text overHeatMessage;
    [Header("Weapon")]
    [FormerlySerializedAs(oldName:"tempSlider")] public Slider temperatureSlider;
    [Header("Player")]
    public Slider playerHealthSlider;
    [Header("Pannel")]
    public GameObject deathPanel;
    public TMP_Text deathText;
    [Space]
    public TMP_Text killsText, deathsText;

    public GameObject leaderBoardPannel;
    public Leaderboard lboardPlayerInfo;

    public GameObject endScreenPannel;
    public TMP_Text timerText;

    public GameObject optionPannel;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }
        if(optionPannel.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowHideOptions()
    {
        if (!optionPannel.activeInHierarchy)
        {
            optionPannel.SetActive(true);
        }
        else
        {
            optionPannel.SetActive(false);
        }
    }

    public void MainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
