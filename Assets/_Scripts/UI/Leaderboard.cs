using UnityEngine;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] TMP_Text playerNameText, playerKillsText, playerDeathsText;

    public void SetDetails(string name,int kills,int deaths)
    {
        playerNameText.text = name;
        playerKillsText.text = kills.ToString();
        playerDeathsText.text = deaths.ToString();
    }
}
