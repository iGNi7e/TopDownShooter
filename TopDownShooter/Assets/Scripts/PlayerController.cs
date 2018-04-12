using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Quaternion targetRotation;

    public float rotationSpeed = 450f;
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    public Gun gun;
    private Camera cam;
    private CharacterController controller;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
    }

    private void Update()
    {
        //Movement();
        ControlMouse();
        if (Input.GetButtonDown("Shoot"))
        {
            gun.Shoot();
        }
        else if(Input.GetButton("Shoot"))
        {
            gun.ShootContinue();
        }
    }

    void ControlMouse()
    {
        Vector3 mousePos = Input.mousePosition; //получение координат положения мыши
        mousePos = cam.ScreenToWorldPoint(new Vector3(mousePos.x,mousePos.y,cam.transform.position.y-transform.position.y)); //преобразование координат в мировые
        targetRotation = Quaternion.LookRotation(mousePos -  new Vector3(transform.position.x,0,transform.position.z)); //кватернион направления поворота
        transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,targetRotation.eulerAngles.y,rotationSpeed * Time.deltaTime); //поворот

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));

        Vector3 motion = input;
        motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1) ? .7f : 1;
        motion *= (Input.GetButton("Run")) ? runSpeed : walkSpeed;
        motion += Vector3.up * -8f;
        controller.Move(motion * Time.deltaTime);
    }


    void Movement()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));

        if (input != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(input);
            transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,targetRotation.eulerAngles.y,rotationSpeed * Time.deltaTime);
        }

        Vector3 motion = input;
        motion *= (Mathf.Abs(input.x) == 1 && Mathf.Abs(input.z) == 1) ? .7f : 1;
        motion *= (Input.GetButton("Run")) ? runSpeed : walkSpeed;
        motion += Vector3.up * -8f;
        controller.Move(motion * Time.deltaTime);
    }
}
