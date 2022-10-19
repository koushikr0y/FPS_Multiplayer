//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float mouseSenstivity = 1f;
    private float verticalRotation;
    private Vector2 mouseInput;
    [SerializeField] private float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    
    [SerializeField] private bool invertLook;
    private Vector3 moveDir,movement;
    [SerializeField] private float jumpForce = 12f,gravityMod = 2.5f;
    private Camera cam;


    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayers;
    public bool isGrounded;
    [SerializeField] private CharacterController charCon;

    [SerializeField] private GameObject bulletImpact;
    private float shotCounter;
    [SerializeField] private float muzzleDisplayTime = 1 / 60f;
    private float muzzleCounter;

    [Header("GUNS OVERHEAT")]
    private float maxHeat = 10f, coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overheated;

    [Header("Gun Manager Details")]
    public GunManager[] Guns;
    private int currentSelectGun;

    [Header("Player")]
    [SerializeField] GameObject playerModel;
    [SerializeField] GameObject playerHitImpact;
    [SerializeField] int maxHealth = 100;
    private int currentHealth;
    [SerializeField] Transform modelGunPoint, gunHolder;


    [SerializeField] Animator anim;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;
        currentHealth = maxHealth; //health of the player   
        if (photonView.IsMine) 
        { 
            playerModel.SetActive(false);
            UIManager.instance.playerHealthSlider.maxValue = maxHealth;
            UIManager.instance.playerHealthSlider.value = currentHealth;
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
        UIManager.instance.temperatureSlider.maxValue = maxHeat;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            CameraMovement();
            PlayerMovement();
            #region Cursor mode in build
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
            #endregion

            #region Gun Overheat Check

            if (Guns[currentSelectGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;
                if (muzzleCounter <= 0)
                {
                    Guns[currentSelectGun].muzzleFlash.SetActive(false);
                }
            }
            if (!overheated)
            {
                #region Bullet Shoot
                if (Input.GetMouseButtonDown(0))
                {
                    ShootBullet();
                }
                #endregion

                #region Automatic Firing
                if (Input.GetMouseButton(0) && /*GunController.instance.*/Guns[/*GunController.instance.*/currentSelectGun].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;
                    if (shotCounter <= 0)
                    {
                        ShootBullet();
                    }
                }
                #endregion

                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overheatCoolRate * Time.deltaTime;
                if (heatCounter <= 0)
                {
                    overheated = false;
                    UIManager.instance.overHeatMessage.gameObject.SetActive(false);
                }
            }
            if (heatCounter < 0) { heatCounter = 0; }
            UIManager.instance.temperatureSlider.value = heatCounter;
            #endregion

            #region Switch Guns Scroll
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                currentSelectGun++;
                if (currentSelectGun >= Guns.Length)
                {
                    currentSelectGun = 0;
                }
                //SwitchGuns();
                photonView.RPC("SetGun", RpcTarget.All, currentSelectGun);
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                currentSelectGun--;
                if (currentSelectGun < 0f)
                {
                    currentSelectGun = Guns.Length - 1;
                }
                //SwitchGuns();
                photonView.RPC("SetGun", RpcTarget.All, currentSelectGun);
            }

            //using number
            for (int i = 0; i < Guns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {              
                    currentSelectGun = i;
                    //SwitchGuns();
                    photonView.RPC("SetGun", RpcTarget.All, currentSelectGun);
                }
            }
            #endregion

            anim.SetBool("grounded",isGrounded);
            anim.SetFloat("speed",moveDir.magnitude);
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            cam.transform.position = viewPoint.position;
            cam.transform.rotation = viewPoint.rotation;
        }
    }

    private void CameraMovement()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSenstivity;
        //left right rotation
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
        //up and down rotation - fix
        verticalRotation += mouseInput.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);
        if (invertLook)
        {
            viewPoint.rotation = Quaternion.Euler(verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        else
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotation, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
    }

    private void PlayerMovement()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f,Input.GetAxisRaw("Vertical"));
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
        }
        else
        {
            activeMoveSpeed = moveSpeed;
        }

        float yVel = movement.y;
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
        movement.y = yVel;
        
        PlayerJump();
        
        charCon.Move(movement * Time.deltaTime);

    }

    private void PlayerJump()
    {
        if (charCon.isGrounded)
        {
            movement.y = 0f;
        }

        isGrounded = Physics.Raycast(groundCheck.position,Vector3.down,1f,groundLayers); //.25f

        if (Input.GetButtonDown("Jump") && isGrounded /*charCon.isGrounded*/)
        {
            movement.y = jumpForce;
        }

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
    }

    private void ShootBullet()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray.origin = cam.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.CompareTag("Player")) 
            { 
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, Guns[currentSelectGun].shotDamage);
            }
            else
            {
                //hit
                GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObject, 2f);
            }
        }

        shotCounter = Guns[currentSelectGun].timeBetweenShots;

        heatCounter += Guns[currentSelectGun].heatPerShot;
        if (heatCounter >= maxHeat)   //checking maxheat 
        {
            heatCounter = maxHeat;

            overheated = true;
            UIManager.instance.overHeatMessage.gameObject.SetActive(true);
        }
        Guns[currentSelectGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    [PunRPC] //ShootBullet()
    private void DealDamage(string damagerName,int damageAmount)
    {
        TakeDamage(damagerName,damageAmount);
    }

    private void TakeDamage(string damagerName,int damageAmount)
    {
        if (photonView.IsMine) 
        {
            currentHealth -= damageAmount;
            if (currentHealth <= 0) 
            {
                currentHealth = 0; 
                PlayerSpawn.instance.PlayerDie(damagerName); 
            }
            UIManager.instance.playerHealthSlider.value = currentHealth;
        }
    }

    [PunRPC]
    private void SetGun(int gunToSwitch)
    {
        if(gunToSwitch < Guns.Length)
        {
            currentSelectGun = gunToSwitch;
            SwitchGuns();
        }
    }

    private void SwitchGuns()
    {
        foreach (GunManager gun in Guns)
        {
            gun.gameObject.SetActive(false);
        }
        Guns[currentSelectGun].gameObject.SetActive(true);
        Guns[currentSelectGun].muzzleFlash.SetActive(false); //after switching guns the muzzle flush should be deactivate
    }

}
