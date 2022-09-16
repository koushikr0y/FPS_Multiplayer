//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public static GunController instance;
    public GunManager[] Guns;
    //public GunDetails[] gunDetails;
    public int currentSelectGun;
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

    public void SwitchGuns()
    {
        foreach (GunManager gun in Guns)
        {
            gun.gameObject.SetActive(false);
        }
        Guns[currentSelectGun].gameObject.SetActive(true);
        Guns[currentSelectGun].muzzleFlash.SetActive(false); //after switching guns the muzzle flush should be deactivate
    }
}

//public class GunDetails
//{
//    public bool isAutomatic;
//    public float timeBetweenShots = 0.1f, heatPerShot = 1f;
//}
