using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private bool isMoving = false;
    private bool isGrounded = false;
    [SerializeField] private float runSpeed = 5;
    [SerializeField] private float jumpHeight = 5;
    [SerializeField] private float mouseSens = 250f;
    [SerializeField] private TextMeshProUGUI gameEndText;
    private CharacterController characterController;
    private float yRotation;
    Animator animator;
    private Vector3 velocity;
    private Vector3 gravityDir = Vector3.down;
    private float gravity = 9.81f;
    Vector3 nextGravityDir = Vector3.down;
    private float timeToDeath = 10f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }   

    private void Update()
    {
        HandleCharacterMovement();
        HandleMouseMovement();
        HanldeGravity();
        HandleJump();
        UpdateAnimator();
        ChangeGravityDirection();
        CheckFreeFall();
    }

   

    private void HandleCharacterMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        characterController.Move(move * runSpeed * Time.deltaTime);

        isMoving = move.magnitude > 0.1f;
    }

    private void HandleMouseMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;

        yRotation += mouseX;

        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    private void HanldeGravity()
    {

        RaycastHit hit;
        if (Physics.Raycast(transform.position, gravityDir, out hit, 0.2f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        velocity += gravityDir * gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity = -gravityDir * jumpHeight;
        }
    }


    


    private void UpdateAnimator()
    {
        if (isGrounded && isMoving)
        {
            animator.SetBool("isRunning", true);
        }
        else 
        {
            animator.SetBool("isRunning", false);
        }
    }

    private void ChangeGravityDirection()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            nextGravityDir = Vector3.left;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            nextGravityDir = Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            nextGravityDir = Vector3.up;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            nextGravityDir = Vector3.down;
        }
        if(Input.GetKeyDown(KeyCode.Return))
        {
            gravityDir = nextGravityDir.normalized;
        }
        
    }
    private void CheckFreeFall()
    {
        
        if (!isGrounded)
        {
            Debug.Log(timeToDeath);
            timeToDeath -= 1f * Time.deltaTime;
            if(timeToDeath < 0)
            {
                gameEndText.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
                
        }
        else
        {
            timeToDeath = 10;
        }
    }

}
