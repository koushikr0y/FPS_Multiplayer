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
    [FormerlySerializedAs(oldName:"tempSlider")]public Slider temperatureSlider;
    private void Awake()
    {
        instance = this; 
    }
}
