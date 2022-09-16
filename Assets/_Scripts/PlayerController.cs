//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    //private float timeBetweenShots = 0.1f;
    private float shotCounter;
    [SerializeField] private float muzzleDisplayTime = 1 / 60f;
    private float muzzleCounter;

    //overheat
    private float maxHeat = 10f,/* heatperShot = 1f,*/ coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool overheated;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        UIManager.instance.temperatureSlider.maxValue = maxHeat;
        GunController.instance.SwitchGuns();
        Transform newPos =  SpawnManager.instance.GetRandomSpawnPositions();
        transform.position = newPos.position;
        transform.rotation = newPos.rotation;
    }

    void Update()
    {
        CameraMovement();
        PlayerMovement();
        #region Cursor mode in build
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        #endregion

        #region Gun Overheat Check

        if (GunController.instance.Guns[GunController.instance.currentSelectGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0)
            {
                GunController.instance.Guns[GunController.instance.currentSelectGun].muzzleFlash.SetActive(false);
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
            if (Input.GetMouseButton(0) && GunController.instance.Guns[GunController.instance.currentSelectGun].isAutomatic)
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
            GunController.instance.currentSelectGun++;
            if (GunController.instance.currentSelectGun >= GunController.instance.Guns.Length)
            {
                GunController.instance.currentSelectGun = 0;
            }
            GunController.instance.SwitchGuns();
            Debug.Log(" csddd" +GunController.instance.currentSelectGun);

        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            GunController.instance.currentSelectGun--;
            if (GunController.instance.currentSelectGun < 0f)
            {
                GunController.instance.currentSelectGun = GunController.instance.Guns.Length-1;
            }
            GunController.instance.SwitchGuns();
        }

        //using number
        for(int i=0;i < GunController.instance.Guns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString())) {
                GunController.instance.currentSelectGun = i;
                GunController.instance.SwitchGuns(); }
        }
        #endregion
    }

    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
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
        
        charCon.Move(movement * Time.deltaTime);

        PlayerJump();
    }

    private void PlayerJump()
    {
        if (charCon.isGrounded)
        {
            movement.y = 0f;
        }

        //isGrounded = Physics.Raycast(groundCheck.position,Vector3.down,0.25f,groundLayers);

        if (Input.GetButtonDown("Jump") && charCon.isGrounded)
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
            //hit
            GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
            Destroy(bulletImpactObject, 2f);
        }

        shotCounter = GunController.instance.Guns[GunController.instance.currentSelectGun].timeBetweenShots;

        heatCounter += GunController.instance.Guns[GunController.instance.currentSelectGun].heatPerShot;
        if (heatCounter >= maxHeat)   //checking maxheat 
        {
            heatCounter = maxHeat;

            overheated = true;
            UIManager.instance.overHeatMessage.gameObject.SetActive(true);
        }
        GunController.instance.Guns[GunController.instance.currentSelectGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }
}
