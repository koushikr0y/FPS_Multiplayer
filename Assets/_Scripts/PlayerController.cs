//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float mouseSenstivity = 1f;
    private float verticalRot;
    private Vector2 mouseInput;
    
    void Start()
    {
        
    }

    void Update()
    {
        mouseInput = new Vector2(Input.GetAxisRaw(""), Input.GetAxisRaw(""));
    }
}
