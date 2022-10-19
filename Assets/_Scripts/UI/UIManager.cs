//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;
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
}
